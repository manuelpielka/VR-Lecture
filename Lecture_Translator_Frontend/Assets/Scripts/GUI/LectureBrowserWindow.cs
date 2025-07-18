using BrowsingModule;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GUI
{
    public class LectureBrowserWindow : Window
    {
        private bool onlineMode;

        private string currentPath;

        private const string rootPath = "/";

        [SerializeField] private TextMeshProUGUI pathTextBox;

        [SerializeField] private GameObject lectureUIPrefab;

        [SerializeField] private GameObject folderUIPrefab;

        [SerializeField] private Transform uiParent;

        [SerializeField] private BrowsingManager browsingManager;

        /// <summary>
        /// Selects a lecture to begin playback with.
        /// </summary>
        /// <param name="lecture"> The lecture that should be played.</param>
        public void SelectLecture(Lecture lecture)
        {
            Window playbackWindow = WindowManager.OpenWindow(WindowKeys.LecturePlayerKey);

            playbackWindow.GetComponent<LecturePlayerWindow>().AssignLecture(lecture);
        }

        /// <summary>
        /// Selects a lecture to download.
        /// </summary>
        /// <param name="lecture"> The lecture that should be downloaded.</param>
        public void DownloadLecture(Lecture lecture)
        {
            LectureDownloader.DownloadLecture(lecture);
        }

        public void ChangeCurrentPath(string newPath)
        {
            currentPath = newPath;
            pathTextBox.text = currentPath.Replace("//", "/").Replace("/", " > ");

            LoadLectures(currentPath);
        }

        public void LoadLectures(string path)
        {
            foreach (Transform child in uiParent)
            {
                Destroy(child.gameObject);
            }

            List<GenericElement> allElements = browsingManager.GetContents(path);

            foreach (var element in allElements)
            {
                if (element is FolderElement)
                {
                    GameObject instance = Instantiate(folderUIPrefab, uiParent);

                    FolderUI folderUI = instance.GetComponent<FolderUI>();

                    folderUI.SetValues(element.GetPath(), element.GetName(), this);
                }
                else if (element is LectureElement)
                {
                    LectureElement lectureElement = (LectureElement)element;

                    GameObject instance = Instantiate(lectureUIPrefab, uiParent);

                    LectureUI lectureUI = instance.GetComponent<LectureUI>();

                    lectureUI.SetValues(element.GetName(), null, "Testdate", "testlength", lectureElement.GetLecture(), this); //TODO: get real metadata
                }
            }
        }

        public void SearchTextInputEnded(string value)
        {
            List<GenericElement> searchElements = browsingManager.Search(value);

            foreach (Transform child in uiParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var element in searchElements)
            {
                if (element is FolderElement)
                {
                    GameObject instance = Instantiate(folderUIPrefab, uiParent);

                    FolderUI folderUI = instance.GetComponent<FolderUI>();

                    folderUI.SetValues("/", "Test", this);
                }
                else if (element is LectureElement)
                {
                    LectureElement lectureElement = (LectureElement)element;

                    GameObject instance = Instantiate(lectureUIPrefab, uiParent);

                    LectureUI lectureUI = instance.GetComponent<LectureUI>();

                    lectureUI.SetValues("Test", null, "Testdate", "testlength", lectureElement.GetLecture(), this); //TODO: get real metadata
                }
            }
        }
    }
}
