namespace MindfulJournal.Services;

/// <summary>
/// Manages the application theme settings and persistence.
/// </summary>
public class ThemeService
{
    private const string ThemePreferenceKey = "MindfulJournal_ThemePreference";
    private const string CustomColorKey = "MindfulJournal_CustomColor";
    
    private bool _isDarkMode = true; // Default to dark mode
    private string _customPrimaryColor = "#7B1FA2"; // Default purple
    
    /// <summary>
    /// Occurs when the theme settings change.
    /// </summary>
    public event Action? OnThemeChanged;
    
    /// <summary>
    /// Gets or sets a value indicating whether dark mode is enabled.
    /// </summary>
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                SavePreferences();
                OnThemeChanged?.Invoke();
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the custom primary color for the theme.
    /// </summary>
    public string CustomPrimaryColor
    {
        get => _customPrimaryColor;
        set
        {
            if (_customPrimaryColor != value)
            {
                _customPrimaryColor = value;
                SavePreferences();
                OnThemeChanged?.Invoke();
            }
        }
    }
    
    /// <summary>
    /// Initializes the theme service by loading saved preferences.
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadPreferences();
    }
    
    /// <summary>
    /// Loads theme preferences from secure storage.
    /// </summary>
    private async Task LoadPreferences()
    {
        try
        {
            var themePreference = await SecureStorage.GetAsync(ThemePreferenceKey);
            if (!string.IsNullOrEmpty(themePreference))
            {
                _isDarkMode = themePreference == "dark";
            }
            
            var customColor = await SecureStorage.GetAsync(CustomColorKey);
            if (!string.IsNullOrEmpty(customColor))
            {
                _customPrimaryColor = customColor;
            }
        }
        catch
        {
            // Use defaults if storage fails
        }
    }
    
    /// <summary>
    /// Saves the current theme preferences to secure storage.
    /// </summary>
    private void SavePreferences()
    {
        try
        {
            SecureStorage.SetAsync(ThemePreferenceKey, _isDarkMode ? "dark" : "light");
            SecureStorage.SetAsync(CustomColorKey, _customPrimaryColor);
        }
        catch
        {
            // Ignore save failures
        }
    }
    
    /// <summary>
    /// Toggles the dark mode setting.
    /// </summary>
    public void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
    }
    
    /// <summary>
    /// A collection of preset colors available for selection.
    /// </summary>
    public static readonly Dictionary<string, string> PresetColors = new()
    {
        { "Purple", "#7B1FA2" },
        { "Blue", "#1976D2" },
        { "Teal", "#00796B" },
        { "Green", "#388E3C" },
        { "Orange", "#F57C00" },
        { "Pink", "#C2185B" },
        { "Indigo", "#303F9F" },
        { "Red", "#D32F2F" }
    };
}
