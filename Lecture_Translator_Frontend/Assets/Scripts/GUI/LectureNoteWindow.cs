using UnityEngine;

namespace GUI
{
    public class LectureNoteWindow : MonoBehaviour
    {
        public string lectureName;

        private PlaybackManager playbackManager;

        public void TimeStampClicked(NoteGUI note)
        {
            string timestampString = note.TitleTextBox.text;

            //playbackManager.MoveTo(timestamp); TODO: Use noteutils to change timestamp from string to double
        }
    }
}
