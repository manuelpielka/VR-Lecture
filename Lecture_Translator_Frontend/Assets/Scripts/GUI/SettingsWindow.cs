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

    public Button resetTutorialButton;
    public Button applyButton;
    public Button discardButton;

    //public SettingsManager settingsManager;

    private readonly string[] supportedLanguages = { "English", "Deutsch"};

    // Temporary cached values for Apply/Discard logic
    private bool tempAutoAdjust;
    private bool tempDarkMode;
    private int tempBackgroundIndex;
    private int tempLanguageIndex;


    void Start()
    {
        Debug.Log("SettingsWindow Started");

        //if (settingsManager == null)
        //{
           // settingsManager = FindFirstObjectByType<SettingsManager>();
           // if (settingsManager == null)
           // {
               // Debug.LogError("SettingsManager not assigned in the scene.");
               // return;
           //}
        //}

        //InitDropdowns();
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

        //backgroundDropdown.ClearOptions();
        //var envManager = FindAnyObjectByType<EnvironmentManager>();
        //if (envManager != null)
        //{
            //foreach (var env in envManager.Environments)
                //backgroundDropdown.options.Add(new TMP_Dropdown.OptionData(env.DisplayName));
        //}

        //languageDropdown.ClearOptions();
        //foreach (var lang in supportedLanguages)
            //languageDropdown.options.Add(new TMP_Dropdown.OptionData(lang));
    }

    private void LoadInitialSettings()
    {

        tempAutoAdjust = SettingsManager.Instance.GetAutoSwitch();
        tempDarkMode = SettingsManager.Instance.GetDarkMode();
        //tempBackgroundIndex = GetBackgroundIndex(UserPreferencesManager.LoadBackgroundSceneId());
        //tempLanguageIndex = GetLanguageIndex(UserPreferencesManager.LoadLanguage());

        modeAutoSwitchToggle.isOn = tempAutoAdjust;
        darkModeToggle.isOn = tempDarkMode;

        //backgroundDropdown.value = tempBackgroundIndex;
        //languageDropdown.value = tempLanguageIndex;
        UpdateToggleInteractableStates();
    }



    //private int GetBackgroundIndex(string sceneId)
    //{
        //var envManager = FindAnyObjectByType<EnvironmentManager>();
        //if (envManager == null) return 0;
        //return envManager.Environments.FindIndex(e => e.SceneId == sceneId);
    //}

    //private int GetLanguageIndex(string lang)
    //{
        //for (int i = 0; i < supportedLanguages.Length; i++)
            //if (supportedLanguages[i] == lang) return i;
        //return 0;
    //}

    private void BindListeners()
    {
        modeAutoSwitchToggle.onValueChanged.AddListener(OnAutoSwitchToggleChanged);
        darkModeToggle.onValueChanged.AddListener(OnDarkModeToggleChanged);
        applyButton.onClick.AddListener(OnApplyButtonClicked);
        discardButton.onClick.AddListener(OnDiscardButtonClicked);


        //backgroundDropdown.onValueChanged.AddListener(index =>
        //{
            //tempBackgroundIndex = index;
        //});

        //languageDropdown.onValueChanged.AddListener(index =>
        //{
            //tempLanguageIndex = index;
        //});
    }

    private void OnAutoSwitchToggleChanged(bool isOn)
    {
        tempAutoAdjust = isOn;
        if (isOn) tempDarkMode = false;
        UpdateToggleStates();
    }

    private void OnDarkModeToggleChanged(bool isOn)
    {
        tempDarkMode = isOn;
        if (isOn) tempAutoAdjust = false;
        UpdateToggleStates();
    }

    /// <summary>
    /// 根据当前状态刷新 toggle 状态和交互性
    /// </summary>
    private void UpdateToggleStates()
    {
        modeAutoSwitchToggle.isOn = tempAutoAdjust;
        darkModeToggle.isOn = tempDarkMode;

        UpdateToggleInteractableStates();
    }

    /// <summary>
    /// 控制互斥关系（互相禁止点击）
    /// </summary>
    private void UpdateToggleInteractableStates()
    {
        modeAutoSwitchToggle.interactable = !tempDarkMode;
        darkModeToggle.interactable = !tempAutoAdjust;
    }

    public void OnApplyButtonClicked()
    {
        SettingsManager.Instance.ApplySettings(tempAutoAdjust, tempDarkMode);
        Debug.Log("[SettingsWindow] Apply pressed: saved & applied.");
    }

    public void OnDiscardButtonClicked()
    {
        LoadInitialSettings();
        Debug.Log("[SettingsWindow] Discard pressed: reverted changes.");
    }
}
