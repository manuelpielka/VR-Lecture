using System.Collections.Generic;
using System.Linq;
using Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI window for application settings, such as:
/// - Display mode (auto-switch vs dark mode)
/// - Language selection
/// - Environment/scene selection
/// - Resetting tutorials
///
/// Provides Apply/Discard logic so changes are not committed
/// until the user presses the <see cref="applyButton"/>.
/// </summary>
public class SettingsWindow : Window
{
    
   
    public TMP_Dropdown backgroundDropdown;
    public Toggle modeAutoSwitchToggle;
    public Toggle darkModeToggle;
    public TMP_Dropdown languageDropdown;

    [SerializeField] private Button resetTutorialButton;
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

    /// <summary>
    /// Unity lifecycle method. Initializes the window, loads preferences,
    /// builds dropdown options, and binds UI listeners.
    /// Also ensures the Reset Tutorial button is correctly wired.
    /// </summary>
    void Start()
    {
        Debug.Log("SettingsWindow Started");

        LoadInitialSettings();
        BuildLanguageDropdown();
        BuildEnvironmentDropdown();
        BindListeners();

        if (resetTutorialButton == null)
            resetTutorialButton = GetComponentInChildren<Button>(includeInactive: true);

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

    /// <summary>
    /// Event handler for the "Reset Tutorial" button.
    /// Fires a global signal so the tutorial system can reset its state.
    /// </summary>
    private void OnResetTutorialClicked()
    {
        Debug.Log("Reset Tutorial Button Clicked");
        Signals.TutorialResetRequested?.Invoke();
    }

    /// <summary>
    /// Loads saved settings (auto adjust, dark mode, etc.)
    /// from <see cref="SettingsManager"/> into the window state.
    /// </summary>
    private void LoadInitialSettings()
    {

        tempAutoAdjust = SettingsManager.Instance.GetAutoSwitch();
        tempDarkMode = SettingsManager.Instance.GetDarkMode();

        modeAutoSwitchToggle.isOn = tempAutoAdjust;
        darkModeToggle.isOn = tempDarkMode;

        UpdateToggleInteractableStates();
    }

    /// <summary>
    /// Builds the language dropdown based on <see cref="SettingsManager"/> data.
    /// </summary>
    private void BuildLanguageDropdown()
    {
        var dict = SettingsManager.Instance.GetLanguages(); 
        languageCodes = dict.Keys.OrderBy(k => dict[k]).ToList();
        languageNames = languageCodes.Select(code => dict[code]).ToList();

        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(languageNames);

        SyncLanguageDropdownToCurrent();
        pendingLanguageCode = null;
    }

    /// <summary>
    /// Syncs the language dropdown value to the current language code.
    /// </summary>
    private void SyncLanguageDropdownToCurrent()
    {
        string current = SettingsManager.Instance.GetCurrentLanguageCode();
        int idx = Mathf.Max(0, languageCodes.IndexOf(current));
        languageDropdown.SetValueWithoutNotify(idx);
        languageDropdown.RefreshShownValue();
    }

    /// <summary>
    /// Builds the environment dropdown based on <see cref="SettingsManager"/> data.
    /// </summary>
    private void BuildEnvironmentDropdown()
    {
        envOptions = SettingsManager.Instance.GetEnvironmentOptions();

        backgroundDropdown.ClearOptions();
        backgroundDropdown.AddOptions(envOptions);

        SyncEnvironmentDropdownToCurrent();
        pendingEnv = null;
    }

    /// <summary>
    /// Syncs the environment dropdown to the currently active environment.
    /// </summary>
    private void SyncEnvironmentDropdownToCurrent()
    {
        string current = SettingsManager.Instance.GetCurrentEnvironment();
        int idx = Mathf.Max(0, envOptions.IndexOf(current));
        backgroundDropdown.SetValueWithoutNotify(idx);
        backgroundDropdown.RefreshShownValue();
    }

    /// <summary>
    /// Binds listeners to UI elements (toggles, buttons, dropdowns).
    /// Ensures user interaction updates temporary state before Apply.
    /// </summary>
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

    /// <summary>
    /// Handles auto-switch toggle changes and updates state.
    /// </summary>
    private void OnAutoSwitchToggleChanged(bool isOn)
    {
        tempAutoAdjust = isOn;
        if (isOn) tempDarkMode = false;
        UpdateToggleStates();
    }

    /// <summary>
    /// Handles dark mode toggle changes and updates state.
    /// </summary>
    private void OnDarkModeToggleChanged(bool isOn)
    {
        tempDarkMode = isOn;
        if (isOn) tempAutoAdjust = false;
        UpdateToggleStates();
    }

    /// <summary>
    /// Applies temporary toggle states to the actual UI elements.
    /// </summary>
    private void UpdateToggleStates()
    {
        modeAutoSwitchToggle.isOn = tempAutoAdjust;
        darkModeToggle.isOn = tempDarkMode;

        UpdateToggleInteractableStates();
    }

    /// <summary>
    /// Updates toggle interactable states to enforce mutual exclusivity.
    /// </summary>

    private void UpdateToggleInteractableStates()
    {
        modeAutoSwitchToggle.interactable = !tempDarkMode;
        darkModeToggle.interactable = !tempAutoAdjust;
    }

    /// <summary>
    /// Event handler for the Apply button.
    /// Commits pending values to <see cref="SettingsManager"/> and applies changes.
    /// </summary>
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

    /// <summary>
    /// Event handler for the Discard button.
    /// Reverts all unsaved changes and reloads settings from <see cref="SettingsManager"/>.
    /// </summary>
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
