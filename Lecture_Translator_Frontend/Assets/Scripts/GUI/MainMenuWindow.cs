using UnityEngine;

namespace GUI
{
    /// <summary>
    /// This Window is used for opening the lecture browser, the notes and the settings.
    /// </summary>
    public class MainMenuWindow : Window
    {
        /// <summary>
        /// Button handler for opening the Lecture Browser Window.
        /// </summary>
        public void OpenLectureBrowser()
        {
            WindowManager.OpenWindow(WindowKeys.LectureBrowserKey);
        }

        /// <summary>
        /// Button handler for opening the Note Window.
        /// </summary>
        public void OpenNoteWindow()
        {
            WindowManager.OpenWindow(WindowKeys.NotesKey);
        }

        /// <summary>
        /// Button handler for opening the Settings Window.
        /// </summary>
        public void OpenSettingsWindow()
        {
            WindowManager.OpenWindow(WindowKeys.SettingsKey);
        }

        void Start()
        {
            if (SessionStateManager.HasPreviousSession())
            {
                WindowManager.CreateWindow(WindowKeys.ContinueWatchingKey);
            }
        }
    }
}
