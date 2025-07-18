using UnityEngine;

namespace BrowsingModule
{
    /// <summary>
    /// Class for representing a lecture in the folder structure.
    /// </summary>
    public class LectureElement : GenericElement
    {
        
        /// <summary>
        /// The Lecture associated with this element.
        /// </summary>
        private Lecture lecture;

        /// <summary>
        /// Constructor for creating a LectureElement.
        /// </summary>
        public LectureElement(string path, string name, Lecture lecture)
        {
            SetPath(path);
            SetName(name);
            this.lecture = lecture;
        }

        /// <summary>
        /// Getter for the lecture attribute.
        /// </summary>
        public Lecture GetLecture()
        {
            return lecture;
        }

    }
}
