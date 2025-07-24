using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace BrowsingModule
{
    /// <summary>
    /// This class handles browsing through lectures.
    /// </summary>
    public class BrowsingManager : MonoBehaviour
    {
        /// <summary>
        /// The root element of the folder structure.
        /// </summary>
        private FolderElement root;

        /// <summary>
        /// The url of the Lecture Translator's archive.
        /// </summary>
        private string archiveAPI = "https://lecture-translator.kit.edu";

        /// <summary>
        /// The path of the root of the Lecture Translator's tree structure.
        /// </summary>
        private const string rootPath = "/";

        /// <summary>
        /// Reference to the LectureBrowserWindow to update the UI.
        /// </summary>
        [SerializeField] private GUI.LectureBrowserWindow lectureBrowserWindow;


        private const string DATA_DIRECTORY = "./Data/";

        private const int requestTimeout = 1;

        /// <summary>
        /// This method is called by unity when this GameObject is enabled.
        /// </summary>
        private void OnEnable()
        {
            root = new FolderElement(rootPath, "root", new List<GenericElement>());
            UpdateFolders();
        }

        /// <summary>
        /// Updates the current folder structure and MetaData and stores it on local drive if internet is availalble.
        /// Reads folder structure from local drive otherwise.
        /// </summary>
        public async void UpdateFolders()
        {
            await GetDir(rootPath, root);

            lectureBrowserWindow.ChangeCurrentPath(rootPath);
        }

        /// <summary>
        /// Gets all elements in the directory and all elements inside folders in this directory from the Lecture Translater's archive.
        /// </summary>
        /// <param name="dir"> The directory to get the elements from. </param>
        /// <param name="parent"> The FolderElement where the found elements are stored in. </param>
        /// <returns> An awaitable task, so the program knows when the method is done. </returns>
        private async Task GetDir(string dir, FolderElement parent)
        {
            string jsonBody = $"\"{{\\\"directory\\\":\\\"{dir}\\\",\\\"groups\\\":[\\\"admin\\\",\\\"kitemployee\\\",\\\"kitall\\\",\\\"all\\\",\\\"basic\\\",\\\"presenter\\\",\\\"collector\\\"]}}\"";
            Task<string> requestTask = PostRequest(archiveAPI + "/ltarchive/ls", jsonBody);
            string result = await requestTask;

            print(result);

            if (result == "")
            {
                // Offline
                lectureBrowserWindow.onlineMode = false;
                GetDirOffline(DATA_DIRECTORY, parent);
                return;
            }

            List<string> itemnames = GetFolderNames(result);
            itemnames.Sort();

            foreach (var item in itemnames)
            {
                FolderElement newElement;

                if (item == " Back")
                {
                    string path = dir;
                    int lastSlash = path.LastIndexOf('/'); // Remove the last slash and everything afterwards so the path is the parent path

                    if (lastSlash >= 0)
                    {
                        path = path.Substring(0, lastSlash);
                    }

                    newElement = new FolderElement(path, item, new List<GenericElement>());
                    parent.AddContents(newElement);
                    continue;
                }
                newElement = new FolderElement(dir + "/" + item, item, new List<GenericElement>());
                
                parent.AddContents(newElement);

                GetDir(dir + "/" + item, newElement);
            }

            List<string> sessionnames = GetSessionNames(result);
            sessionnames.Sort();

            foreach (var session in sessionnames)
            {
                Lecture lecture = await LectureDownloader.DownloadMetaData(dir + "/" + session);
                LectureElement newSessionElement = new LectureElement(dir + "/" + session, session, lecture);

                parent.AddContents(newSessionElement);
            }
        }

        private void GetDirOffline(string dir, FolderElement parent)
        {
            //print(dir);

            if (dir != DATA_DIRECTORY)
            {
                // Add back button

                string path = dir;
                int lastSlash = path.LastIndexOf('/'); // Remove the last slash and everything afterwards so the path is the parent path

                if (lastSlash >= 0)
                {
                    path = path.Substring(0, lastSlash);
                }

                FolderElement backElement = new FolderElement(path, " Back", new List<GenericElement>());
                parent.AddContents(backElement);
            }

            if (Directory.Exists(dir))
            {
                string[] files = Directory.GetFiles(dir);
                string[] directories = Directory.GetDirectories(dir);

                //Debug.Log("Directories:");
                foreach (string directory in directories)
                {
                    string fixedDir = directory.Replace("\\", "/");
                    //Debug.Log(fixedDir);

                    string dirName = fixedDir.Substring(fixedDir.LastIndexOf("/") + 1);

                    FolderElement newElement = new FolderElement(fixedDir, dirName, new List<GenericElement>());

                    parent.AddContents(newElement);

                    GetDirOffline(fixedDir, newElement);
                }

                //Debug.Log("Files:");
                foreach (string file in files)
                {
                    string fixedFile = file.Replace("\\", "/");

                    //Debug.Log(fixedFile);

                    string fileName = fixedFile.Substring(fixedFile.LastIndexOf("/") + 1);

                    Lecture lecture = new Lecture(fileName, fixedFile, "", null);
                    lecture.SetDownloaded(true);
                    LectureElement newSessionElement = new LectureElement(fixedFile, fileName, lecture);

                    parent.AddContents(newSessionElement);
                }
            }
            else
            {
                Debug.LogError("Folder not found: " + dir);
            }
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
            request.timeout = requestTimeout;

            //print("Sending request to: " + url);

            await request.SendWebRequest();

            return request.downloadHandler.text;
        }

        /// <summary>
        /// Helping function that converts json data into a List of folder names.
        /// </summary>
        /// <param name="json"> The json data to convert. </param>
        /// <returns> A List of folder names. </returns>
        private List<string> GetFolderNames(string json)
        {
            var result = new List<string>();

            var matches = Regex.Matches(json, @"\[\s*""([^""]+)""\s*,\s*""([^""]+)""");

            foreach (Match match in matches)
            {
                string first = match.Groups[1].Value;
                string second = match.Groups[2].Value;

                if (second == "dir")
                {
                    result.Add(first);
                }
            }

            return result;
        }

        /// <summary>
        /// Helping function that converts json data into a List of session names.
        /// </summary>
        /// <param name="json"> The json data to convert. </param>
        /// <returns> A List of session names. </returns>
        private List<string> GetSessionNames(string json)
        {
            var result = new List<string>();

            var matches = Regex.Matches(json, @"\[\s*""([^""]+)""\s*,\s*""([^""]+)""");

            foreach (Match match in matches)
            {
                string first = match.Groups[1].Value;
                string second = match.Groups[2].Value;

                if (second == "session")
                {
                    result.Add(first);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the contents of the folder at the given path.
        /// </summary>
        /// <param name="path"> The path to get the contents from. </param>
        /// <returns> List of the contents of the path. </returns>
        public List<GenericElement> GetContents(string path)
        {
            string[] paths = path.Split("/");

            FolderElement currElement = root;

            foreach (string elementName in paths)
            {
                List<GenericElement> elements = currElement.GetContents();

                foreach (GenericElement element in elements)
                {
                    if (element.GetName() == elementName)
                    {
                        currElement = (FolderElement)element;
                    }
                }
            }

            return currElement.GetContents();
        }

        /// <summary>
        /// Returns a list of search results including both folders and lectures for the given name at the given path.
        /// </summary>
        /// <param name="name"> The name to search for. </param>
        /// <param name="path"> The path to search in. </param>
        /// <returns> A list of search results. </returns>
        public List<GenericElement> Search(string name, string path)
        {
            string[] paths = path.Split("/");

            FolderElement currElement = root;

            foreach (string elementName in paths)
            {
                List<GenericElement> elements = currElement.GetContents();

                foreach (GenericElement element in elements)
                {
                    if (element.GetName() == elementName)
                    {
                        currElement = (FolderElement)element;
                    }
                }
            }

            return Search(name, currElement);
        }

        /// <summary>
        /// Returns a list of search results including both folders and lecture for the given name inside of a FolderElement.
        /// </summary>
        /// <param name="name"> The name to search for. </param>
        /// <param name="currFolder"> The folder to search in. </param>
        /// <returns> A list of search results. </returns>
        private List<GenericElement> Search(string name, FolderElement currFolder)
        {
            List<GenericElement> returnList = new List<GenericElement>();

            foreach (GenericElement element in currFolder.GetContents())
            {
                if (element.GetName().ToLower().Contains(name.ToLower()))
                {
                    returnList.Add(element);
                }

                if (element is FolderElement) // Is this a folder?
                {
                    List<GenericElement> newList = Search(name, (FolderElement)element);
                    foreach (GenericElement newElement in newList)
                    {
                        returnList.Add(newElement);
                    }
                }

            }
            return returnList;
        }
    }
}
