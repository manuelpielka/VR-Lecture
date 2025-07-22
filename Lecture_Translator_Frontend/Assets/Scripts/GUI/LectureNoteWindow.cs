using System;
using UnityEngine;
using TMPro;

namespace GUI
{
    public class LectureNoteWindow : NoteWindow
    {
        private string lectureName;

        private PlaybackManager playbackManager;

        [SerializeField] private TextMeshProUGUI titleTextbox;


        public void SetValues(string lectureName, PlaybackManager playbackManager)
        {
            this.lectureName = lectureName;
            this.playbackManager = playbackManager;

            titleTextbox.text = "Notes for Lecture: " + lectureName;
        }

        public void TimeStampClicked(NoteGUI note)
        {
            string timestampString = note.TitleTextBox.text;

            TimeSpan timeSpan = TimeSpan.Parse(timestampString);

            playbackManager.MoveTo(timeSpan.TotalSeconds);
        }
    }
}
