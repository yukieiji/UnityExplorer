using UnityExplorer.Config;
using UnityEngine; // Added for PlayerPrefs
using System.Collections.Generic; // Added for Dictionary

namespace UnityExplorer.Translation;

public sealed partial class TranslationManager
{
    public enum Lang
    {
        English,
        Japanese
    }

    private const string PlayerPrefsKey_LastLang = "UnityExplorer_LastSelectedLanguage";
    private static IReadOnlyDictionary<string, string> activeLocalizedData; // Changed type here

    // This Initialize method should be called once from an external source (e.g., ExplorerCore.Init())
    public static void Initialize()
    {
        LoadLastLanguage();
        // Subscribe to language change events from ConfigManager
        ConfigManager.Lang_Toggle.OnValueChanged += SetActiveLanguage;
    }

    private static void LoadLastLanguage()
    {
        // Default to English if no preference is saved
        Lang lastLang = (Lang)PlayerPrefs.GetInt(PlayerPrefsKey_LastLang, (int)Lang.English);
        // Update the ConfigManager's value to reflect the loaded language.
        // This also triggers SetActiveLanguage via OnValueChanged if the current value is different.
        ConfigManager.Lang_Toggle.Value = lastLang;
        // Ensure activeLocalizedData is set correctly on initial load, even if Lang_Toggle.Value was already lastLang
        if (activeLocalizedData == null && lastLang != Lang.English)
        {
             SetActiveLanguage(lastLang);
        }
        else if (lastLang == Lang.English) // Ensure English clears activeLocalizedData
        {
            activeLocalizedData = null;
        }
    }

    private static void SetActiveLanguage(Lang lang)
    {
        ExplorerCore.Log($"UnityExplorer: Setting active language to {lang}");
        if (lang == Lang.English)
        {
            activeLocalizedData = null;
        }
        else if (lang == Lang.Japanese)
        {
            activeLocalizedData = JapaneseStrings.Data;
        }
        // Add cases for other languages here
        else
        {
            activeLocalizedData = null; // Fallback to English if language not supported
            ExplorerCore.LogWarning($"UnityExplorer: Language {lang} is not currently supported, falling back to English.");
        }
        PlayerPrefs.SetInt(PlayerPrefsKey_LastLang, (int)lang);
    }

    // Public Get method that uses the current language from ConfigManager
    public static string Get(string key)
    {
        return Get(ConfigManager.Lang_Toggle.Value, key);
    }

    // Core Get method
    public static string Get(Lang lang, string key)
    {
        string value = null;

        if (lang != Lang.English && activeLocalizedData != null)
        {
            activeLocalizedData.TryGetValue(key, out value);
        }

        // If not found in the localized dictionary (or if English is selected), try English dictionary
        if (value == null)
        {
            if (EnglishStrings.Data != null && EnglishStrings.Data.TryGetValue(key, out string englishValue))
            {
                value = englishValue;
            }
        }

        return value ?? key; // If still not found, return the key itself
    }

    // Get method with formatting
    public static string Get(string key, params object[] parts)
    {
        string row_val = Get(key); // This will call Get(ConfigManager.Lang_Toggle.Value, key)
        try
        {
            return string.Format(row_val, parts);
        }
        catch (System.FormatException ex)
        {
            ExplorerCore.LogWarning($"UnityExplorer: Error formatting translation for key '{key}'. Raw value: '{row_val}'. Error: {ex.Message}");
            return row_val; // Return raw value if formatting fails
        }
    }
}
