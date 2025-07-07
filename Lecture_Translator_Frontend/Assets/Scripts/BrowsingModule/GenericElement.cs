using UnityEngine;

namespace BrowsingModule
{
    /// <summary>
    /// Abstract class for representing the folder structure.
    /// </summary>
    public abstract class GenericElement
    {
        /// <summary>
        /// The path in the folder structure for this element.
        /// </summary>
        private string path;

        /// <summary>
        /// The name for this element.
        /// </summary>
        private string name;

        /// <summary>
        /// Getter for the path attribute.
        /// </summary>
        public string GetPath()
        {
            return path;
        }

        /// <summary>
        /// Getter for the name attribute.
        /// </summary>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        /// Setter for the path attribute.
        /// </summary>
        public void SetPath(string _path)
        {
            path = _path;
        }

        /// <summary>
        /// Setter for the name attribute.
        /// </summary>
        public void SetName(string _name)
        {
            name = _name;
        }
    }
}
