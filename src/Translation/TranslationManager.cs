using UnityExplorer.Config;

#nullable enable

namespace UnityExplorer.Translation;

public sealed partial class TranslationManager
{
    public enum Lang
    {
        English,
        Japanese
    }

    private static readonly Translator translator = new Translator();

    public static string Get(string key)
        => Get(ConfigManager.Lang_Toggle.Value, key);

    public static string Get(Lang lang, string key)
        => translator.Get(lang, key);

    public static string Get(string key, params object[] parts)
    {
        string raw = Get(key);
        try
        {
            return string.Format(raw, parts);
        }
        catch (FormatException ex)
        {
            ExplorerCore.LogWarning($"UnityExplorer: Error formatting translation for key '{key}'. Raw value: '{raw}'. Error: {ex.Message}");
            return raw;
        }
    }
}
