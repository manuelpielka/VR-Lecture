using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LecturePlayerWindow : Window
{
    [SerializeField] private TextMeshProUGUI videoProgressTextBox;
    [SerializeField] private Slider videoSlider;
    [SerializeField] private SliderSelectHandler sliderSelectHandler;
    [SerializeField] private TextMeshProUGUI subtitleSizeTextBox;
    [SerializeField] private TMP_Dropdown playBackSpeedDropdown;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private TextMeshProUGUI subtitleTextBox;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private TranscriptManager transcriptManager;
    [SerializeField] private SubtitleManager subtitleManager;
    [SerializeField] private PlaybackManager playbackManager;
    private Lecture lecture;
    private const float SKIP_AMOUNT = 30;

    void Update()
    {
        //Update current line
        subtitleTextBox.text = subtitleManager.getCurrentLine();


        double value = playbackManager.GetCurrentTime();


        //update time displays
        videoSlider.maxValue = (float)playbackManager.GetVideoLength();
        if (!sliderSelectHandler.IsSelected)
        {
            videoSlider.value = (float)value;
        }


        //format as string
        int hours = (int)(value / 3600);
        string stringHours = hours.ToString();
        if (stringHours.Length < 2)
        {
            stringHours = "0" + stringHours;
        }
        value -= hours * 3600;

        int minutes = (int)(value / 60);
        string stringMinutes = minutes.ToString();
        if (stringMinutes.Length < 2)
        {
            stringMinutes = "0" + stringMinutes;
        }
        value -= minutes * 60;

        int seconds = (int)value;
        string stringSeconds = seconds.ToString();
        if (stringSeconds.Length < 2)
        {
            stringSeconds = "0" + stringSeconds;
        }

        string formattedTime = stringHours + ":" + stringMinutes + ":" + stringSeconds;
        videoProgressTextBox.text = formattedTime;

    }

    //make sure streamed data is deleted if appliction is quit without having closed the PlaybackWindow
    void OnApplicationQuit()
    {
        try
        {
            if (!lecture.IsDownloaded())
            {
                File.Delete(playbackManager.VideoPlayer.url);
            }
        }
        catch (Exception)
        {
            //might occur if file has not been downloaded or no lecture has benn assigned yet when the application is closed.
            //if the application is closing anyway then there is no issue with this exception, it just means there is nothing to delete
        }
        
    }

    public void AssignLecture(Lecture lecture)
    {
        this.lecture = lecture;
        transcriptManager.AssignLecture(lecture);
        playbackManager.AssignLecture(lecture);

        languageDropdown.AddOptions(lecture.GetTranscriptLanguages());
    }

    public void Play()
    {
        playbackManager.Play();
    }

    public void Pause()
    {
        playbackManager.Pause();
    }

    public void SkipForward()
    {
        if (playbackManager.GetVideoLength() < playbackManager.GetCurrentTime() + SKIP_AMOUNT)
        {
            playbackManager.MoveTo(playbackManager.GetVideoLength());
        }
        playbackManager.MoveTo(playbackManager.GetCurrentTime() + SKIP_AMOUNT);
    }

    public void SkipBackward()
    {
        if (playbackManager.GetCurrentTime() < -SKIP_AMOUNT)
        {
            playbackManager.MoveTo(0);
        }
        playbackManager.MoveTo(playbackManager.GetCurrentTime() - SKIP_AMOUNT);
    }

    public void VideoSliderChanged()
    {
        if (sliderSelectHandler.IsSelected)
        {
            playbackManager.MoveTo(videoSlider.value);
        }

    }

    public void AiBtnPressed()
    {
        Window window = WindowManager.CreateWindow(WindowKeys.DialogueKey);

        window.GetComponent<DialogueController>().SetContentDirectory(lecture.GetTranscriptSource());
    }

    public void TranscriptBtnPressed()
    {
        TranscriptWindow window = (TranscriptWindow)WindowManager.CreateWindow(WindowKeys.TranscriptKey);
        window.AssignLecture(lecture);
    }

    public void NotesBtnPressed()
    {
        var window = WindowManager.CreateWindow(WindowKeys.CreateLectureNoteKey) as CreateLectureNoteWindow;
        if (window == null)
        {
            Debug.LogError("Failed to open CreateLectureNoteWindow.");
            return;
        }

        double now = playbackManager.GetCurrentTime();

        if (lecture != null)
        {
            
            window.Initialize(lecture, now, isEditMode: false);
        }
        else
        {
            
            window.Initialize("(Lecture deleted)", now, isEditMode: false);
        }
    }

    public void SettingBtnPressed()
    {
        if (settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
        }
        else
        {
            settingsPanel.SetActive(true);
        }
    }

    public void ContinueWatchingYesBtnPressed()
    {
        playbackManager.MoveTo(lecture.GetLastPlayTime());
    }

    public void ContinueWatchingNoBtnPressed()
    {
        playbackManager.MoveTo(0);
    }

    public void SubtitleSizePlusBtnPressed()
    {
        subtitleTextBox.fontSize++;
        subtitleSizeTextBox.text = subtitleTextBox.fontSize.ToString();
    }

    public void SubtitleSizeMinusBtnPressed()
    {
        subtitleTextBox.fontSize--;
        subtitleSizeTextBox.text = subtitleTextBox.fontSize.ToString();
    }

    public void PlayBackSpeedSelected()
    {
        string selectedOption = playBackSpeedDropdown.options[playBackSpeedDropdown.value].text;
        float value = float.Parse(selectedOption);
        playbackManager.SetPlaybackSpeed(value);
    }

    public void LanguageSelected()
    {
        string selectedOption = languageDropdown.options[languageDropdown.value].text;
        subtitleManager.SetLanguage(selectedOption);
    }

    public void CloseBtnPressed()
    {
        lecture.SetLastPlayTime(playbackManager.GetCurrentTime());

        //delete downloaded file if "streaming"
        if (!lecture.IsDownloaded())
        {
            File.Delete(playbackManager.VideoPlayer.url);
        }

        Close();
    }

}
