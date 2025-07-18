using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsWindow : Window
{
    public TMP_Dropdown playbackSpeedDropdown;
    public TMP_Dropdown subtitleSizeDropdown;
    public Toggle autoAdjustToggle;
    public Toggle darkModeToggle;
    public SettingsManager settingsManager;

    // magic numbers??
    private readonly float[] playbackSpeeds = { 1.0f, 1.25f, 1.5f, 1.75f, 2.0f };
    private readonly int[] subtitleSizes = { 12, 14, 16, 18, 20 };

    void Start()
    {
        InitDropdowns();
        InitToggles();
        BindListeners();
    }

    private void InitDropdowns()
    {
        playbackSpeedDropdown.ClearOptions();
        foreach (var speed in playbackSpeeds)
        {
            playbackSpeedDropdown.options.Add(new TMP_Dropdown.OptionData(speed.ToString("0.##") + "x"));
        }

        float currentSpeed = settingsManager.GetComponent<PlaybackSettingsManager>().GetPlaybackSpeed();
        int selectedIndex = System.Array.IndexOf(playbackSpeeds, currentSpeed);
        playbackSpeedDropdown.value = selectedIndex >= 0 ? selectedIndex : 0;

        subtitleSizeDropdown.ClearOptions();
        foreach (var size in subtitleSizes)
        {
            subtitleSizeDropdown.options.Add(new TMP_Dropdown.OptionData(size.ToString()));
        }

        int currentSize = settingsManager.GetComponent<PlaybackSettingsManager>().GetSubtitleFontSize();
        int selectedSizeIndex = System.Array.IndexOf(subtitleSizes, currentSize);
        subtitleSizeDropdown.value = selectedSizeIndex >= 0 ? selectedSizeIndex : 2; // default = 16
    }

    private void InitToggles()
    {
        bool isAutoAdjust = settingsManager.GetComponent<DisplayModeController>().IsAutoAdjustEnabled();
        bool isDark = settingsManager.GetComponent<DisplayModeController>().IsDarkModeEnabled();

        autoAdjustToggle.isOn = isAutoAdjust;
        darkModeToggle.isOn = isDark;
        darkModeToggle.interactable = !isAutoAdjust;
    }

    private void BindListeners()
    {

    }
}
