using System.Collections.Generic;
using System.Linq;
using Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWindow : Window
{
    
   
    public TMP_Dropdown backgroundDropdown;
    public Toggle modeAutoSwitchToggle;
    public Toggle darkModeToggle;
    public TMP_Dropdown languageDropdown;

    public Button resetTutorialButton;
    public Button applyButton;
    public Button discardButton;

    // Temporary cached values for Apply/Discard logic
    private bool tempAutoAdjust;
    private bool tempDarkMode;

    private List<string> languageCodes = new List<string>();   // "en","de"
    private List<string> languageNames = new List<string>();
    private string pendingLanguageCode = null;

    private List<string> envOptions = new List<string>();
    private string pendingEnv = null;


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
        BuildLanguageDropdown();
        BuildEnvironmentDropdown();
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
        Signals.TutorialResetRequested?.Invoke();
       
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

    private void BuildLanguageDropdown()
    {
        var dict = SettingsManager.Instance.GetLanguages(); // code -> display
        languageCodes = dict.Keys.OrderBy(k => dict[k]).ToList();
        languageNames = languageCodes.Select(code => dict[code]).ToList();

        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(languageNames);

        SyncLanguageDropdownToCurrent();
        pendingLanguageCode = null;
    }

    private void SyncLanguageDropdownToCurrent()
    {
        string current = SettingsManager.Instance.GetCurrentLanguageCode();
        int idx = Mathf.Max(0, languageCodes.IndexOf(current));
        languageDropdown.SetValueWithoutNotify(idx);
        languageDropdown.RefreshShownValue();
    }

    private void BuildEnvironmentDropdown()
    {
        envOptions = SettingsManager.Instance.GetEnvironmentOptions();

        backgroundDropdown.ClearOptions();
        backgroundDropdown.AddOptions(envOptions);

        SyncEnvironmentDropdownToCurrent();
        pendingEnv = null;
    }

    private void SyncEnvironmentDropdownToCurrent()
    {
        string current = SettingsManager.Instance.GetCurrentEnvironment();
        int idx = Mathf.Max(0, envOptions.IndexOf(current));
        backgroundDropdown.SetValueWithoutNotify(idx);
        backgroundDropdown.RefreshShownValue();
    }

    private void BindListeners()
    {
        modeAutoSwitchToggle.onValueChanged.AddListener(OnAutoSwitchToggleChanged);
        darkModeToggle.onValueChanged.AddListener(OnDarkModeToggleChanged);
        applyButton.onClick.AddListener(OnApplyButtonClicked);
        discardButton.onClick.AddListener(OnDiscardButtonClicked);

        languageDropdown.onValueChanged.AddListener(idx =>
        {
            if (idx >= 0 && idx < languageCodes.Count)
                pendingLanguageCode = languageCodes[idx];
        });


        backgroundDropdown.onValueChanged.AddListener(index =>
        {
            if (index >= 0 && index < envOptions.Count)
                pendingEnv = envOptions[index];
        });
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

    private void UpdateToggleStates()
    {
        modeAutoSwitchToggle.isOn = tempAutoAdjust;
        darkModeToggle.isOn = tempDarkMode;

        UpdateToggleInteractableStates();
    }


    private void UpdateToggleInteractableStates()
    {
        modeAutoSwitchToggle.interactable = !tempDarkMode;
        darkModeToggle.interactable = !tempAutoAdjust;
    }

    public async void OnApplyButtonClicked()
    {
        SettingsManager.Instance.ApplySettings(tempAutoAdjust, tempDarkMode);

        string current = SettingsManager.Instance.GetCurrentLanguageCode();
        string target = string.IsNullOrEmpty(pendingLanguageCode) ? current : pendingLanguageCode;

        if (!string.IsNullOrEmpty(target) && target != current)
        {
            await SettingsManager.Instance.ApplyLanguageAsync(target);
        }

        pendingLanguageCode = null;
        SyncLanguageDropdownToCurrent();

        if (!string.IsNullOrEmpty(pendingEnv) && pendingEnv != SettingsManager.Instance.GetCurrentEnvironment())
        {
            SettingsManager.Instance.ApplyEnvironment(pendingEnv);
        }
        pendingEnv = null;
        SyncEnvironmentDropdownToCurrent();

        Debug.Log("[SettingsWindow] Apply pressed: saved & applied.");
        Debug.Log($"[Lang] current={SettingsManager.Instance.GetCurrentLanguageCode()} pending={pendingLanguageCode}");
    }

    public void OnDiscardButtonClicked()
    {
        LoadInitialSettings();

        pendingLanguageCode = null;
        SyncLanguageDropdownToCurrent();

        pendingEnv = null;
        SyncEnvironmentDropdownToCurrent();

        Debug.Log("[SettingsWindow] Discard pressed: reverted changes.");
    }
}
