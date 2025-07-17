using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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

        private string archiveAPI = "https://lecture-translator.kit.edu";

        public string hierachy = "";

        private void OnEnable()
        {
            root = new FolderElement("/", "root", new List<GenericElement>());
            UpdateFolders();
        }

        /// <summary>
        /// Updates the current folder structure and MetaData and stores it on local drive if internet is availalble.
        /// Reads folder structure from local drive otherwise.
        /// </summary>
        public void UpdateFolders()
        {
            // TODO: implement
            string dir = "/";
            GetDir(dir, root);
        }

        private async void GetDir(string dir, FolderElement parent)
        {
            string jsonBody = $"\"{{\\\"directory\\\":\\\"{dir}\\\",\\\"groups\\\":[\\\"admin\\\",\\\"kitemployee\\\",\\\"kitall\\\"]}}\"";
            Task<string> requestTask = PostRequest(archiveAPI + "/ltarchive/ls", jsonBody);
            string result = await requestTask;

            print(result);

            List<string> itemnames = GetFolderNames(result);

            foreach (var item in itemnames)
            {
                if (item == " Back") continue;

                hierachy += "v " + item + "\n";
                print(item);
                FolderElement newElement = new FolderElement(dir, item, new List<GenericElement>());
                parent.AddContents(newElement);

                GetDir(dir + "/" + item, newElement);
            }
        }

        private async Task<string> PostRequest(string url, string json)
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] byteJson = new UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(byteJson);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            print("Sending request to: " + url);

            await request.SendWebRequest();

            return request.downloadHandler.text;
        }

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
                if (second == "session")
                {

                }
            }

            return result;
        }

        /// <summary>
        /// Returns the contents of the folder at the given path.
        /// </summary>
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
        /// Returns a list of search results including both folders and lectures for the given name.
        /// </summary>
        public List<GenericElement> Search(string name)
        {
            List<GenericElement> searchList = Search(name, root);

            return searchList;
        }

        private List<GenericElement> Search(string name, FolderElement currFolder)
        {
            List<GenericElement> returnList = new List<GenericElement>();

            foreach (GenericElement element in currFolder.GetContents())
            {
                if (element.GetName() == name)
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
