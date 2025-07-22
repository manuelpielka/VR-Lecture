using System;
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

            TimeSpan timeSpan = TimeSpan.Parse(timestampString);

            playbackManager.MoveTo(timeSpan.TotalSeconds);
        }
    }
}
