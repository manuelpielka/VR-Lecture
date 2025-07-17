using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class LecturePlayerWindow : Window
{
    [SerializeField] private TextMeshProUGUI videoProgressTextBox;
    [SerializeField] private Slider videoSlider;
    [SerializeField] private Toggle subtitleToggle;
    [SerializeField] private TextMeshProUGUI subtitleSizeTextBox;
    [SerializeField] private TMP_Dropdown playBackSpeedDropdown;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private TextMeshProUGUI subtitleTextBox;
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

        //TODO: need to retrieve languages and assign them to the dropdown here
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

    public void VideoSliderChanged(float value)
    {
        playbackManager.MoveTo(playbackManager.GetVideoLength() * value);
    }

    public void AiBtnPressed()
    {
        //not yet implemented
    }

    public void TranscriptBtnPressed()
    {
        //not yet implemented
    }

    public void NotesBtnPressed()
    {
        //not yet implemented
    }

    public void SettingBtnPressed()
    {
        //not yet implemented
    }

    [SerializeField]
    private void ContinueWatchingYesBtnPressed()
    {
        playbackManager.MoveTo(lecture.GetLastPlayTime());
    }

    private void ContinueWatchingNoBtnPressed()
    {
        playbackManager.MoveTo(0);
    }

    private void SubtitlesToggled(bool value)
    {
        if (value)
        {
            subtitleTextBox.alpha = 255;
        }
        else
        {
            subtitleTextBox.alpha = 0;
        }
    }

    private void SubtitleSizePlusBtnPressed()
    {
        subtitleTextBox.fontSize++;
        subtitleSizeTextBox.text = subtitleTextBox.fontSize.ToString();
    }

    private void SubtitleSizeMinusBtnPressed()
    {
        subtitleTextBox.fontSize--;
        subtitleSizeTextBox.text = subtitleTextBox.fontSize.ToString();
    }

    private void PlayBackSpeedSelected(string selectedOption)
    {
        float value = float.Parse(selectedOption);
        playbackManager.SetPlaybackSpeed(value);
    }

    private void LanguageSelected(string selectedOption)
    {
        subtitleManager.SetLanguage(selectedOption);
    }
}
