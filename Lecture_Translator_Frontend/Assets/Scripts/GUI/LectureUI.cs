using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GUI
{
    /// <summary>
    /// A utility class for depicting the lecture UI element displayed in the Lecture Browser Window.
    /// </summary>
    public class LectureUI : MonoBehaviour
    {
        /// <summary>
        /// The lecture associated with this object.
        /// </summary>
        private Lecture lecture;

        /// <summary>
        /// Reference to the lecture browser window to call methods.
        /// </summary>
        private LectureBrowserWindow lectureBrowserUI;

        /// <summary>
        /// The textbox for the title of the lecture.
        /// </summary>
        [SerializeField] private TextMeshProUGUI titleTextBox;

        /// <summary>
        /// The textbox for the date and length of the lecture.
        /// </summary>
        [SerializeField] private TextMeshProUGUI dateTextBox;

        /// <summary>
        /// The preview image of the lecture.
        /// </summary>
        [SerializeField] private Image thumbnailDisplay;


        /// <summary>
        /// Button handler for the play button.
        /// </summary>
        public void OnClick()
        {
            lectureBrowserUI.SelectLecture(lecture);
        }

        /// <summary>
        /// Button handler for the download button.
        /// </summary>
        public void OnDownloadClick()
        {
            lectureBrowserUI.DownloadLecture(lecture);
        }

        /// <summary>
        /// Sets the values of this object in one method.
        /// </summary>
        /// <param name="title"> The title of the lecture. </param>
        /// <param name="thumbnail"> The thumbnail of the lecture. </param>
        /// <param name="date"> The date of the lecture. </param>
        /// <param name="length"> The length of the lecture. </param>
        /// <param name="_lecture"> The lecture element of the lecture. </param>
        /// <param name="_lectureBrowserUI"> The reference of the lecture browser window. </param>
        public void SetValues(string title, Sprite thumbnail, string date, string length, Lecture _lecture, LectureBrowserWindow _lectureBrowserUI)
        {
            titleTextBox.text = title;
            thumbnailDisplay.sprite = thumbnail;
            dateTextBox.text = date + " - " + length;
            lecture = _lecture;
            lectureBrowserUI = _lectureBrowserUI;
        }

    }
}
