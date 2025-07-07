using System.Collections.Generic;
using UnityEngine;


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
        /// Updates the current folder structure and MetaData and stores it on local drive if internet is availalble.
        /// Reads folder structure from local drive otherwise.
        /// </summary>
        public void UpdateFolders()
        {
            // TODO: implement
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
