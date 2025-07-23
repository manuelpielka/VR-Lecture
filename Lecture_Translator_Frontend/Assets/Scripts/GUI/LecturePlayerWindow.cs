using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    void Start()
    {
        //TEMPORARY
        test();

    }

    void Update()
    {
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

    private async void test()
    {
        Lecture lecture = await LectureDownloader.DownloadMetaData("Other/offline_test");
        AssignLecture(lecture);
    }

    public void AssignLecture(Lecture lecture)
    {
        this.lecture = lecture;
        transcriptManager.AssignLecture(lecture);
        playbackManager.AssignLecture(lecture);

        languageDropdown.AddOptions(lecture.GetTranscriptLanguages());
    }

    public void SetSubtitleText(string text)
    {
        subtitleTextBox.text = text;
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
    }

    public void TranscriptBtnPressed()
    {
        Window window = WindowManager.CreateWindow(WindowKeys.TranscriptKey);
    }

    public void NotesBtnPressed()
    {
        Window window = WindowManager.CreateWindow(WindowKeys.NotesKey);
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
        float currentTime = (float)playbackManager.GetCurrentTime();

        SessionStateManager.SaveSessionState(lecture, currentTime);
        WindowManager.CloseWindow(this);
        Destroy(gameObject);
    }

    public Lecture GetLecture()
    {
        return this.lecture;
    }

    public PlaybackManager GetPlaybackManager()
    {
        return playbackManager;
    }

    public void SetSubtitleFontSize(int size)
    {
        subtitleTextBox.fontSize = size;
        subtitleSizeTextBox.text = size.ToString();
    }

}
