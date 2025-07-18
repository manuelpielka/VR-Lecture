using UnityEngine;
using TMPro;

namespace GUI
{
    public class FolderUI : MonoBehaviour
    {
        private string path;

        /// <summary>
        /// The textbox for the title of the folder.
        /// </summary>
        [SerializeField] private TextMeshProUGUI titleTextBox;

        /// <summary>
        /// Reference to the lecture browser window to call methods.
        /// </summary>
        private LectureBrowserWindow lectureBrowserUI;

        /// <summary>
        /// Button handler for clicking this folder.
        /// </summary>
        public void OnClick()
        {
            lectureBrowserUI.ChangeCurrentPath(path);
        }

        /// <summary>
        /// Sets the values of the folder ui element.
        /// </summary>
        /// <param name="_path"> The path of this folder. </param>
        /// <param name="title"> The title of this folder. </param>
        /// <param name="_lectureBrowserUI"> The reference to the lecture browser window. </param>
        public void SetValues(string _path, string title, LectureBrowserWindow _lectureBrowserUI)
        {
            path = _path;
            titleTextBox.text = title;
            lectureBrowserUI = _lectureBrowserUI;
        }
    }
}
