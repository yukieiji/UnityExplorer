using System.Collections.Generic;

using UnityExplorer.Config;

namespace UnityExplorer.Translation;

public sealed partial class TranslationManager
{
    public enum Lang
    {
        English,
        Japanese
    }

    private static Dictionary<string, string> en = new()
    {
        { "translation", "UnityExplorer Language" },
        { "translation_hint", "The language of UnityExplorer's menu and features. IF THIS IS CHANGED, REQUIRED REBOOT" },
        { "ue_toggle", "UnityExplorer Toggle" },
        { "ue_toggle_hint", "The key to enable or disable UnityExplorer's menu and features." },
        { "hide_on_startup", "Hide On Startup" },
        { "hide_on_startup_hint", "Should UnityExplorer be hidden on startup?" },
        { "startup_delay_time", "Startup Delay Time" },
        { "startup_delay_time_hint", "The delay on startup before the UI is created." },
        { "target_display", "Target Display" },
        { "target_display_hint", "The monitor index for UnityExplorer to use, if you have multiple. 0 is the default display, 1 is secondary, etc. " +
            "Restart recommended when changing this setting. Make sure your extra monitors are the same resolution as your primary monitor." },
        { "force_unlock_mouse", "Force Unlock Mouse" },
        { "force_unlock_mouse_hint", "Force the Cursor to be unlocked (visible) when the UnityExplorer menu is open." },
        { "force_unlock_toggle_key", "Force Unlock Toggle Key" },
        { "force_unlock_toggle_key_hint", "The keybind to toggle the 'Force Unlock Mouse' setting. Only usable when UnityExplorer is open." },
        { "disable_eventsystem_override", "Disable EventSystem override" },
        { "disable_eventsystem_override_hint", "If enabled, UnityExplorer will not override the EventSystem from the game.\n<b>May require restart to take effect.</b>" },
        { "default_output_path", "The delay on startup before the UI is created." },
        { "default_output_path_hint", "The default output path when exporting things from UnityExplorer." },
        { "dnspy_path", "dnSpy Path" },
        { "dnspy_path_hint", "The full path to dnSpy.exe (64-bit)." },
        { "main_navbar_anchor", "Main Navbar Anchor" },
        { "main_navbar_anchor_hint", "The vertical anchor of the main UnityExplorer Navbar, in case you want to move it." },
        { "log_unity_debug", "Log Unity Debug" },
        { "log_unity_debug_hint", "Should UnityEngine.Debug.Log messages be printed to UnityExplorer's log?" },
        { "log_to_disk", "Log To Disk" },
        { "log_to_disk_hint", "Should UnityExplorer save log files to the disk?" },
        { "world_mouse_inspect_keybind", "World Mouse-Inspect Keybind" },
        { "world_mouse_inspect_keybind_hint", "Optional keybind to being a World-mode Mouse Inspect." },
        { "ui_mouse_inspect_keybind", "UI Mouse-Inspect Keybind" },
        { "ui_mouse_inspect_keybind_hint", "Optional keybind to begin a UI-mode Mouse Inspect." },
        { "csharp_console_assembly_blacklist", "CSharp Console Assembly Blacklist" },
        { "csharp_console_assembly_blacklist_hint", "Use this to blacklist Assembly names from being referenced by the C# Console. Requires a Reset of the C# Console.\n" +
            "Separate each Assembly with a semicolon ';'." +
            "For example, to blacklist Assembly-CSharp, you would add 'Assembly-CSharp;'" },
        { "member_signature_blacklist", "Member Signature Blacklist" },
        { "member_signature_blacklist_hint", "Use this to blacklist certain member signatures if they are known to cause a crash or other issues.\r\n" +
            "Seperate signatures with a semicolon ';'.\r\n" +
            "For example, to blacklist Camera.main, you would add 'UnityEngine.Camera.main;'" },
    };

    private static Dictionary<string, string> jp = new()
    {
        { "translation", "UnityExplorerの言語" },
        { "translation_hint", "UnityExplorerのメニューや機能の言語。このオプションを変更した場合、再起動が必要" },
        { "ue_toggle", "UnityExplorer Toggle" },
        { "ue_toggle_hint", "The key to enable or disable UnityExplorer's menu and features." },
        { "hide_on_startup", "Hide On Startup" },
        { "hide_on_startup_hint", "Should UnityExplorer be hidden on startup?" },
        { "startup_delay_time", "Startup Delay Time" },
        { "startup_delay_time_hint", "The delay on startup before the UI is created." },
        { "target_display", "Target Display" },
        { "target_display_hint", "The monitor index for UnityExplorer to use, if you have multiple. 0 is the default display, 1 is secondary, etc. " +
            "Restart recommended when changing this setting. Make sure your extra monitors are the same resolution as your primary monitor." },
        { "force_unlock_mouse", "Force Unlock Mouse" },
        { "force_unlock_mouse_hint", "Force the Cursor to be unlocked (visible) when the UnityExplorer menu is open." },
        { "force_unlock_toggle_key", "Force Unlock Toggle Key" },
        { "force_unlock_toggle_key_hint", "The keybind to toggle the 'Force Unlock Mouse' setting. Only usable when UnityExplorer is open." },
        { "disable_eventsystem_override", "Disable EventSystem override" },
        { "disable_eventsystem_override_hint", "If enabled, UnityExplorer will not override the EventSystem from the game.\n<b>May require restart to take effect.</b>" },
        { "default_output_path", "The delay on startup before the UI is created." },
        { "default_output_path_hint", "The default output path when exporting things from UnityExplorer." },
        { "dnspy_path", "dnSpy Path" },
        { "dnspy_path_hint", "The full path to dnSpy.exe (64-bit)." },
        { "main_navbar_anchor", "Main Navbar Anchor" },
        { "main_navbar_anchor_hint", "The vertical anchor of the main UnityExplorer Navbar, in case you want to move it." },
        { "log_unity_debug", "Log Unity Debug" },
        { "log_unity_debug_hint", "Should UnityEngine.Debug.Log messages be printed to UnityExplorer's log?" },
        { "log_to_disk", "Log To Disk" },
        { "log_to_disk_hint", "Should UnityExplorer save log files to the disk?" },
        { "world_mouse_inspect_keybind", "World Mouse-Inspect Keybind" },
        { "world_mouse_inspect_keybind_hint", "Optional keybind to being a World-mode Mouse Inspect." },
        { "ui_mouse_inspect_keybind", "UI Mouse-Inspect Keybind" },
        { "ui_mouse_inspect_keybind_hint", "Optional keybind to begin a UI-mode Mouse Inspect." },
        { "csharp_console_assembly_blacklist", "CSharp Console Assembly Blacklist" },
        { "csharp_console_assembly_blacklist_hint", "Use this to blacklist Assembly names from being referenced by the C# Console. Requires a Reset of the C# Console.\n" +
            "Separate each Assembly with a semicolon ';'." +
            "For example, to blacklist Assembly-CSharp, you would add 'Assembly-CSharp;'" },
        { "member_signature_blacklist", "Member Signature Blacklist" },
        { "member_signature_blacklist_hint", "Use this to blacklist certain member signatures if they are known to cause a crash or other issues.\r\n" +
            "Seperate signatures with a semicolon ';'.\r\n" +
            "For example, to blacklist Camera.main, you would add 'UnityEngine.Camera.main;'" },
    };

    public static string Get(Lang lang, string key)
    {
        var target = lang switch
        {
            Lang.Japanese => jp,
            _ => en,
        };
        if (!target.TryGetValue(key, out string ret))
        {
            return key;
        }
        return ret;
    }
    public static string Get(string key)
        => Get(ConfigManager.Lang_Toggle.Value, key);
}
