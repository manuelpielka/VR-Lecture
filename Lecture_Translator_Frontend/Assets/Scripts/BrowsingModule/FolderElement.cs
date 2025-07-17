using System.Collections.Generic;
using UnityEngine;

namespace BrowsingModule
{
    /// <summary>
    /// Abstract class for representing the folder structure.
    /// </summary>
    public class FolderElement : GenericElement
    {
        /// <summary>
        /// The list of contents of this folder.
        /// </summary>
        private List<GenericElement> contents;


        /// <summary>
        /// Constructor of a FolderElement.
        /// </summary>
        public FolderElement(string path, string name, List<GenericElement> contents)
        {
            SetPath(path);
            SetName(name);
            this.contents = contents;
        }

        /// <summary>
        /// Getter for the contents attribute.
        /// </summary>
        public List<GenericElement> GetContents()
        {
            return contents;
        }

        /// <summary>
        /// Adds a generic element to the contents list.
        /// </summary>
        public void AddContents(GenericElement element)
        {
            contents.Add(element);
        }
    }
}
