using System.IO;
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
        /// The textbox for the date and presenter of the lecture.
        /// </summary>
        [SerializeField] private TextMeshProUGUI dateTextBox;

        /// <summary>
        /// The preview image of the lecture.
        /// </summary>
        [SerializeField] private Image thumbnailDisplay;

        /// <summary>
        /// The button to download this lecture.
        /// </summary>
        [SerializeField] private Button downloadButton;

        /// <summary>
        /// The button to delete this downloaded lecture.
        /// </summary>
        [SerializeField] private Button deleteButton;

        /// <summary>
        /// Directory of the downloaded lectures.
        /// </summary>
        private const string DATA_DIRECTORY = "./Data/";


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
        public async void OnDownloadClick()
        {
            downloadButton.gameObject.SetActive(false);

            await lectureBrowserUI.DownloadLecture(lecture);

            deleteButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Download handler for the delete button.
        /// </summary>
        public void OnDeleteClick()
        {
            File.Delete(DATA_DIRECTORY + lecture.GetTranscriptSource() + ".mp4");
            if (Directory.Exists(DATA_DIRECTORY + lecture.GetTranscriptSource()))
                Directory.Delete(DATA_DIRECTORY + lecture.GetTranscriptSource(), true);

            deleteButton.gameObject.SetActive(false);
            downloadButton.gameObject.SetActive(true);
            lecture.SetDownloaded(false);
        }

        /// <summary>
        /// Sets the values of this object in one method.
        /// </summary>
        /// <param name="title"> The title of the lecture. </param>
        /// <param name="thumbnail"> The thumbnail of the lecture. </param>
        /// <param name="date"> The date of the lecture. </param>
        /// <param name="presenter"> The presenter of the lecture. </param>
        /// <param name="_lecture"> The lecture element of the lecture. </param>
        /// <param name="_lectureBrowserUI"> The reference of the lecture browser window. </param>
        public void SetValues(string title, Sprite thumbnail, string date, string presenter, Lecture _lecture, LectureBrowserWindow _lectureBrowserUI)
        {
            titleTextBox.text = title;
            thumbnailDisplay.sprite = thumbnail;
            dateTextBox.text = date + " | " + presenter;
            lecture = _lecture;
            lectureBrowserUI = _lectureBrowserUI;

            if (lecture.IsDownloaded())
            {
                downloadButton.gameObject.SetActive(false);
                deleteButton.gameObject.SetActive(true);
            }
        }

        public void SetThumbnail(Sprite sprite)
        {
            thumbnailDisplay.sprite = sprite;
        }

    }
}
