using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace UnityExplorer.Translation;

public sealed class Translator
{
    private readonly Dictionary<string, string> enData = EnglishStrings.Data;
    private Dictionary<string, string>? active;
    private TranslationManager.Lang prevLang = TranslationManager.Lang.English;

    public string Get(TranslationManager.Lang lang, string key)
    {
        if (active is null || lang != this.prevLang)
        {
            active = SetUp(lang);
            this.prevLang = lang;
        }
        return active.TryGetValue(key, out string? val) ? val : "";
    }

    private Dictionary<string, string> SetUp(TranslationManager.Lang lang)
        => lang switch
        {
            TranslationManager.Lang.English => enData,
            TranslationManager.Lang.Japanese => JapaneseStrings.Data,
            _ => enData,
        };
}
