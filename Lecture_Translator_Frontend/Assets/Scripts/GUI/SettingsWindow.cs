using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SettingsWindow : Window
{
    
   
    public TMP_Dropdown backgroundDropdown;
    public Toggle modeAutoSwitchToggle;
    public Toggle darkModeToggle;
   
    public TMP_Dropdown languageDropdown;
    public TMP_Dropdown playbackSpeedDropdown;
    // public TMP_Dropdown subtitleSizeDropdown;

    public SettingsManager settingsManager;

    public Button resetTutorialButton;

    private readonly float[] playbackSpeeds = { 1.0f, 1.25f, 1.5f, 1.75f, 2.0f };
    //private readonly int[] subtitleSizes = { 16, 18, 20, 22, 24 };
    private readonly string[] supportedLanguages = { "English"};

    // Temporary cached values for Apply/Discard logic
    private bool tempAutoAdjust;
    private bool tempDarkMode;
    private int tempPlaybackIndex;
    private int tempBackgroundIndex;
    private int tempLanguageIndex;


    void Start()
    {
        Debug.Log("SettingsWindow Started");

        if (settingsManager == null)
        {
            settingsManager = FindFirstObjectByType<SettingsManager>();
            if (settingsManager == null)
            {
                Debug.LogError("SettingsManager not assigned in the scene.");
                return;
            }
        }

        InitDropdowns();
        LoadInitialSettings();
        BindListeners();

        resetTutorialButton = transform.Find("Canvas/Panel/ResetTutorialButton")?.GetComponent<Button>();

        if (resetTutorialButton != null)
        {
            Debug.Log("Reset Tutorial Button found and listening.");
            resetTutorialButton.onClick.AddListener(OnResetTutorialClicked);
        }
        else
        {
            Debug.LogWarning("Reset Tutorial Button not found in SettingsWindow.");
        }    
       
    }

    private void OnResetTutorialClicked()
    {
        Debug.Log("Reset Tutorial Button Clicked");

        TutorialManager tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (tutorialManager != null)
        {
            Debug.Log("Reset Tutorial called.");
            tutorialManager.ResetTutorial();
        }
        else
        {
            Debug.LogWarning("TutorialManager not found.");
        }
       
    }

    private void InitDropdowns()
    {
        playbackSpeedDropdown.ClearOptions();
        foreach (var speed in playbackSpeeds)
            playbackSpeedDropdown.options.Add(new TMP_Dropdown.OptionData(speed.ToString("0.##") + "x"));

        //subtitleSizeDropdown.ClearOptions();
        //foreach (var size in subtitleSizes)
        //{
        //subtitleSizeDropdown.options.Add(new TMP_Dropdown.OptionData(size.ToString()));
        //}

        //int currentSize = settingsManager.GetComponent<PlaybackSettingsManager>().GetSubtitleFontSize();
        //int selectedSizeIndex = System.Array.IndexOf(subtitleSizes, currentSize);
        //subtitleSizeDropdown.value = selectedSizeIndex >= 0 ? selectedSizeIndex : 2; // default = 16

        backgroundDropdown.ClearOptions();
        var envManager = FindAnyObjectByType<EnvironmentManager>();
        if (envManager != null)
        {
            foreach (var env in envManager.Environments)
                backgroundDropdown.options.Add(new TMP_Dropdown.OptionData(env.DisplayName));
        }

        languageDropdown.ClearOptions();
        foreach (var lang in supportedLanguages)
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(lang));
    }

    private void LoadInitialSettings()
    {

        tempAutoAdjust = UserPreferencesManager.LoadAutoAdjust();
        tempDarkMode = UserPreferencesManager.LoadDarkMode();
        tempPlaybackIndex = GetPlaybackIndex(UserPreferencesManager.LoadPlaybackSpeed());
        tempBackgroundIndex = GetBackgroundIndex(UserPreferencesManager.LoadBackgroundSceneId());
        //tempLanguageIndex = GetLanguageIndex(UserPreferencesManager.LoadLanguage());

        modeAutoSwitchToggle.isOn = tempAutoAdjust;
        darkModeToggle.isOn = tempDarkMode;
        darkModeToggle.interactable = !tempAutoAdjust;
        playbackSpeedDropdown.value = tempPlaybackIndex;
        backgroundDropdown.value = tempBackgroundIndex;
        languageDropdown.value = tempLanguageIndex;
    }

    private int GetPlaybackIndex(float speed)
    {
        for (int i = 0; i < playbackSpeeds.Length; i++)
            if (Mathf.Approximately(playbackSpeeds[i], speed)) return i;
        return 0;
    }

    private int GetBackgroundIndex(string sceneId)
    {
        var envManager = FindAnyObjectByType<EnvironmentManager>();
        if (envManager == null) return 0;
        return envManager.Environments.FindIndex(e => e.SceneId == sceneId);
    }

    //private int GetLanguageIndex(string lang)
    //{
        //for (int i = 0; i < supportedLanguages.Length; i++)
            //if (supportedLanguages[i] == lang) return i;
        //return 0;
    //}

    private void BindListeners()
    {
        modeAutoSwitchToggle.onValueChanged.AddListener(isOn =>
        {
            tempAutoAdjust = isOn;
            darkModeToggle.interactable = !isOn;
            if (isOn) darkModeToggle.isOn = false;
        });

        darkModeToggle.onValueChanged.AddListener(isOn =>
        {
            tempDarkMode = isOn;
        });

        playbackSpeedDropdown.onValueChanged.AddListener(index =>
        {
            tempPlaybackIndex = index;
        });

        backgroundDropdown.onValueChanged.AddListener(index =>
        {
            tempBackgroundIndex = index;
        });

        languageDropdown.onValueChanged.AddListener(index =>
        {
            tempLanguageIndex = index;
        });
    }

    public void OnApplyPressed()
    {
        settingsManager.SetAutoAdjust(tempAutoAdjust);
        if (!tempAutoAdjust)
        {
            settingsManager.SetDarkMode(tempDarkMode);
        }
        settingsManager.SetPlaybackSpeed(playbackSpeeds[tempPlaybackIndex]);

        var envManager = FindAnyObjectByType<EnvironmentManager>();
        if (envManager != null && envManager.Environments.Count > tempBackgroundIndex)
        {
            var env = envManager.Environments[tempBackgroundIndex];
            settingsManager.SetEnvironmentById(env.SceneId);
        }

        //UserPreferencesManager.SaveLanguage(supportedLanguages[tempLanguageIndex]);

        DisplayModeController.Instance.RefreshMode();
        Debug.Log($"Apply: AutoAdjust={tempAutoAdjust}, Dark={tempDarkMode}, Speed={playbackSpeeds[tempPlaybackIndex]}, BG={tempBackgroundIndex}, Lang={tempLanguageIndex}");
    }

    public void OnDiscardPressed()
    {
        LoadInitialSettings();
    }

    //private void InitToggles()
    //{
        //bool isAutoAdjust = settingsManager.GetComponent<DisplayModeController>().IsAutoAdjustEnabled();
        //bool isDark = settingsManager.GetComponent<DisplayModeController>().IsDarkModeEnabled();

        //modeAutoSwitchToggle.isOn = isAutoAdjust;
        //darkModeToggle.isOn = isDark;
        //darkModeToggle.interactable = !isAutoAdjust;
    //}

    //private void OnPlaybackSpeedChanged(int index)
    //{
        //float selectedSpeed = playbackSpeeds[index];
        //settingsManager.SetPlaybackSpeed(selectedSpeed);
    //}

    //private void OnSubtitleSizeChanged(int index)
    //{
        //int selectedSize = subtitleSizes[index];
        //settingsManager.SetSubtitleFontSize(selectedSize);
    //}

    //private void OnAutoAdjustToggled(bool isOn)
    //{
        //settingsManager.SetAutoAdjust(isOn);
        //darkModeToggle.interactable = !isOn;
        //if (isOn)
        //{
            //DisplayModeController.Instance.RefreshMode();
        //}
        
    //}

    //private void OnDarkModeToggled(bool isOn)
    //{
        //settingsManager.SetDarkMode(isOn);
        //DisplayModeController.Instance.SetDarkMode(isOn);
    //}

    //private void OnBackgroundChanged(int index)
    //{
        //var envManager = FindAnyObjectByType<EnvironmentManager>();
        //if (envManager == null) return;

        //var config = envManager.Environments[index];
        //envManager.SwitchEnvironment(config);
        //UserPreferencesManager.SaveBackgroundSceneId(config.SceneId);
    //}
}
