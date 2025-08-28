using BrowsingModule;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.UI;
using System;
using System.Collections;
using System.IO;

namespace GUI
{
    /// <summary>
    /// The meta data of a lecture from the api.
    /// </summary>
    [System.Serializable]
    public class JsonMetaData
    {
        public string title;
        public string presenter;
        public string eventDate;
    }

    /// <summary>
    /// This Window handles browsing through lectures.
    /// </summary>
    public class LectureBrowserWindow : Window
    {
        /// <summary>
        /// Whether or not the browser is in online mode.
        /// </summary>
        public bool onlineMode = true;

        /// <summary>
        /// The path currently opened.
        /// </summary>
        private string currentPath;

        /// <summary>
        /// The gameobject of the path currently opened.
        /// </summary>
        private GameObject currentPathGO;

        /// <summary>
        /// The textbox where the current path is displayed.
        /// </summary>
        [SerializeField] private TextMeshProUGUI pathTextBox;

        /// <summary>
        /// The textbox where the hierarchy of the current folder is displayed.
        /// </summary>
        [SerializeField] private TextMeshProUGUI hierarchyTextBox;

        /// <summary>
        /// The textbox where the "Browse Online Lectures" text is displayed.
        /// </summary>
        [SerializeField] private TextMeshProUGUI onlineTextBox;

        /// <summary>
        /// The textbox where the "Browse Downloaded Lectures" text is displayed
        /// </summary>
        [SerializeField] private TextMeshProUGUI offlineTextBox;

        /// <summary>
        /// The prefab of a LectureUI element.
        /// </summary>
        [SerializeField] private GameObject lectureUIPrefab;

        /// <summary>
        /// The prefab of a FolderUI element.
        /// </summary>
        [SerializeField] private GameObject folderUIPrefab;

        /// <summary>
        /// The prefab of a parent object used for creating the folder structure.
        /// </summary>
        [SerializeField] private GameObject parentPrefab;

        /// <summary>
        /// The transform of the UIs parent object to instanstiate the ui objects in.
        /// </summary>
        [SerializeField] private Transform uiParent;

        /// <summary>
        /// The transform of the search features parent object to intanstiate the ui objects in.
        /// </summary>
        [SerializeField] private Transform searchParent;

        /// <summary>
        /// Reference to the BrowsingManager.
        /// </summary>
        [SerializeField] private BrowsingManager browsingManager;

        /// <summary>
        /// Reference to the ScrollRect component to change between folders.
        /// </summary>
        [SerializeField] private ScrollRect scrollRect;

        /// <summary>
        /// The url of the Lecture Translator's archive.
        /// </summary>
        private string archiveAPI = "https://lecture-translator.kit.edu/ltarchive/";

        /// <summary>
        /// Name of the api link to get the metadata of a lecture from.
        /// </summary>
        private const string META = "meta";

        /// <summary>
        /// Name of the api link to get the thumbnail of a lecture from.
        /// </summary>
        private const string THUMBNAIL = "thumb";

        private Queue<(byte[] data, Action<Sprite> onReady)> queue = new Queue<(byte[], Action<Sprite>)>();

        bool processing = false;

        [SerializeField] private DownloadProgressBar progressBar;

        /// <summary>
        /// Directory of the downloaded lectures.
        /// </summary>
        private const string DATA_DIRECTORY = "./Data/";


        private void Update()
        {
            if (!processing && queue.Count > 0)
                StartCoroutine(ProcessQueue());
        }

        private IEnumerator ProcessQueue()
        {
            processing = true;

            var (data, onReady) = queue.Dequeue();

            Texture2D texture = new Texture2D(2, 2);
            yield return null;
            texture.LoadImage(data); // This still blocks main thread
            yield return null;
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            onReady?.Invoke(sprite);

            yield return null; // wait 1 frame before processing next
            processing = false;
        }

        /// <summary>
        /// Called when this object is enabled.
        /// </summary>
        private void OnEnable()
        {
            browsingManager.OnOnlineModeChanged += ChangeOnlineMode;
            browsingManager.OnPathChanged += ChangeCurrentPath;
        }

        /// <summary>
        /// Called when this object is disabled.
        /// </summary>
        private void OnDisable()
        {
            browsingManager.OnOnlineModeChanged -= ChangeOnlineMode;
            browsingManager.OnPathChanged -= ChangeCurrentPath;
        }

        /// <summary>
        /// Selects a lecture to begin playback with.
        /// </summary>
        public void SelectLecture(Lecture lecture)
        {
            Window playbackWindow = WindowManager.OpenWindow(WindowKeys.LecturePlayerKey);

            playbackWindow.GetComponent<LecturePlayerWindow>().AssignLecture(lecture);
        }

        /// <summary>
        /// Selects a lecture to download.
        /// </summary>
        /// <param name="lecture"> The lecture that should be downloaded.</param>
        public async Task DownloadLecture(Lecture lecture)
        {
            progressBar.gameObject.SetActive(true);

            await LectureDownloader.DownloadVTT(lecture);
            await LectureDownloader.DownloadLecture(lecture, progressBar);

            lecture.SetDownloaded(true);
        }

        /// <summary>
        /// Deletes all downloaded files of a lecture.
        /// </summary>
        /// <param name="lecture"> The lecture to delete from storage. </param>
        public void DeleteLecture(Lecture lecture)
        {
            File.Delete(DATA_DIRECTORY + lecture.GetTranscriptSource() + ".mp4");
            if (Directory.Exists(DATA_DIRECTORY + lecture.GetTranscriptSource()))
                Directory.Delete(DATA_DIRECTORY + lecture.GetTranscriptSource(), true);
        }

        /// <summary>
        /// Changes the currently selected Path, searches for existing UI data, if it does not exist, it creates the data.
        /// </summary>
        /// <param name="newPath"> The new path. </param>
        public void ChangeCurrentPath(string newPath)
        {
            currentPath = newPath;
            pathTextBox.text = currentPath.Replace("//", "/").Replace("/", " > ");

            searchParent.gameObject.SetActive(false);

            if (currentPathGO != null)
                currentPathGO.SetActive(false);

            Transform newPathGO = uiParent.Find(currentPath.Replace("/", "-"));

            if (newPathGO == null)
            {
                LoadLectures(currentPath);
            }
            else
            {
                newPathGO.gameObject.SetActive(true);
                currentPathGO = newPathGO.gameObject;
                scrollRect.content = (RectTransform) newPathGO;
            }
            UpdateHierarchyText(newPath);
        }

        /// <summary>
        /// Loads all Lectures and Folders, creates UI elements for them and gets the metadata of the Lectures.
        /// </summary>
        /// <param name="path"> The path from which to load elements. </param>
        public void LoadLectures(string path)
        {
            List<GenericElement> allElements = browsingManager.GetContents(path);

            GameObject parent = Instantiate(parentPrefab, uiParent);

            parent.name = path.Replace("/", "-");

            currentPathGO = parent.gameObject;
            scrollRect.content = (RectTransform)parent.transform;

            foreach (var element in allElements)
            {
                if (element is FolderElement)
                {
                    GameObject instance = Instantiate(folderUIPrefab, parent.transform);

                    FolderUI folderUI = instance.GetComponent<FolderUI>();

                    folderUI.SetValues(element.GetPath(), element.GetName(), this);
                }
                else if (element is LectureElement)
                {
                    GetMetaData(element, parent.transform);
                }
            }
        }

        /// <summary>
        /// Gets the metadata of an Lecture and adds it to the UI.
        /// </summary>
        /// <param name="element"> The element to get the metadata from. </param>
        /// <param name="parent"> The parent object to spawn the Ui element in. </param>
        private async void GetMetaData(GenericElement element, Transform parent)
        {
            GameObject instance = Instantiate(lectureUIPrefab, parent);

            LectureElement lectureElement = (LectureElement)element;

            LectureUI lectureUI = instance.GetComponent<LectureUI>();

            if (!onlineMode)
            {
                lectureUI.SetValues(lectureElement.GetName(), null, "", "", lectureElement.GetLecture(), this);
                return;
            }

            string json = $"\"{{\\\"directory\\\":\\\"{element.GetPath()}\\\"}}\"";

            string metaData = await PostRequest(archiveAPI + META, json);

            metaData = metaData.Replace("event", "eventDate"); // event is reserved keyword in c#
            //print(metaData);
            //print("NAME:" + element.GetName());
            //print("PATH:" + element.GetPath());
            JsonMetaData jsonMeta;

            // request the thumbnail, but don't block on it
            Sprite thumbnail = null;
            ImageDownload(archiveAPI + THUMBNAIL, json, sprite => 
            {
                if (instance != null)
                {
                    thumbnail = sprite;
                    lectureUI.SetThumbnail(thumbnail);
                }
            });


            if (instance == null) return; // Object was deleted before the request was answered

            if (metaData == "{}")
            {
                lectureUI.SetValues(element.GetName(), thumbnail, "", "", lectureElement.GetLecture(), this);
                return;
            }

            try
            {
                jsonMeta = JsonUtility.FromJson<JsonMetaData>(metaData);
            }
            catch (System.ArgumentException)
            {
                lectureUI.SetValues(element.GetName(), thumbnail, "", "", lectureElement.GetLecture(), this);
                return;
            }

            lectureUI.SetValues(jsonMeta.title, thumbnail, jsonMeta.eventDate, jsonMeta.presenter, lectureElement.GetLecture(), this);
        }

        /// <summary>
        /// Sends a post request to the url with the json data.
        /// </summary>
        /// <param name="url"> The url to send the request to. </param>
        /// <param name="json"> The data in json format to send. </param>
        /// <returns> The answer of the request. </returns>
        private async Task<string> PostRequest(string url, string json)
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] byteJson = new UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(byteJson);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("Cookie", "_forward_auth=" + Login.token);

            await request.SendWebRequest();

            return request.downloadHandler.text;
        }

        /// <summary>
        /// Sends a post request to the url with the json data in order to download the thumbnail image.
        /// </summary>
        /// <param name="url"> The url to send the request to. </param>
        /// <param name="json"> The data in json format to send. </param>
        /// <returns> The answer of the request converted to a sprite. </returns>
        private async Task ImageDownload(string url, string json, Action<Sprite> onReady)
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] byteJson = new UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(byteJson);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("Cookie", "_forward_auth=" + Login.token);

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            byte[] imageBytes = request.downloadHandler.data;
            queue.Enqueue((imageBytes, onReady));
        }

        /// <summary>
        /// Updates the hierarchy text box with the names of the elements of the given path.
        /// </summary>
        /// <param name="path"> The path of the elements. </param>
        private void UpdateHierarchyText(string path)
        {
            hierarchyTextBox.text = "";

            List<GenericElement> allElements = browsingManager.GetContents(path);

            foreach (var element in allElements)
            {
                if (element.GetName() == " Back") continue;

                hierarchyTextBox.text += "- " + element.GetName() + "\n";
            }
        }

        /// <summary>
        /// Handler for the search input field. Shows all elements that contain the string value in lowered notation.
        /// </summary>
        /// <param name="value"> The string to search for. </param>
        public void SearchTextInputEnded(string value)
        {
            if (value == "") // Cancel search
            {
                searchParent.gameObject.SetActive(false);
                currentPathGO.SetActive(true);
                scrollRect.content = (RectTransform) currentPathGO.transform;
                UpdateHierarchyText(currentPath);
                return;
            }

            List<GenericElement> searchElements = browsingManager.Search(value, currentPath);

            foreach(Transform child in searchParent)
            {
                Destroy(child.gameObject);
            }

            searchParent.gameObject.SetActive(true);
            currentPathGO.SetActive(false);
            scrollRect.content = (RectTransform) searchParent;
            hierarchyTextBox.text = "";

            foreach (var element in searchElements)
            {
                if (element is FolderElement)
                {
                    if (element.GetName() == " Back") continue;

                    GameObject instance = Instantiate(folderUIPrefab, searchParent);

                    FolderUI folderUI = instance.GetComponent<FolderUI>();

                    folderUI.SetValues(element.GetPath(), element.GetName(), this);
                }
                else if (element is LectureElement)
                {
                    GetMetaData(element, searchParent);
                }
                hierarchyTextBox.text += "- " + element.GetName() + "\n";
            }
        }

        /// <summary>
        /// Changes the online mode.
        /// </summary>
        /// <param name="value"> New value. </param>
        public void ChangeOnlineMode(bool value)
        {
            onlineMode = value;

            onlineTextBox.gameObject.SetActive(onlineMode);
            offlineTextBox.gameObject.SetActive(!onlineMode);
        }
    }
}
