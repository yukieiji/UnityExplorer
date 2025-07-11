using System.Collections.Generic;

using System.Collections.Generic; // Ensure IReadOnlyDictionary is available

namespace UnityExplorer.Translation
{
    public static class EnglishStrings
    {
        public static readonly IReadOnlyDictionary<string, string> Data = new Dictionary<string, string>()
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
            { "clipboard", "Clipboard" },
            { "copied", "Copied!" },
            { "pasted", "Pasted!" },
            { "cannot_inspect", "Cannot inspect a null or destroyed object!" },
            { "cannot_assign", "Cannot assign '{0}' to '{1}'!" },
            { "current_paste", "Current paste:" },
            { "clear_clipboard", "Clear Clipboard" },
            { "not_set", "not set" },
            { "inspect", "Inspect" },
            // { "not_set", "not set" }, // Duplicate key, assuming the first one is intended or this one needs a different key. For now, commented out.
            { "autocompleter", "AutoCompleter" },
            { "help_updown_esc", "Up/Down to select, Enter to use, Esc to close" },

            // CSConsolePanel.cs
            { "panel_name_csharp_console", "C# Console" },
            { "button_compile", "Compile" },
            { "button_reset", "Reset" },
            { "dropdown_help", "Help" },
            { "toggle_compile_on_ctrl_r", "Compile on Ctrl+R" },
            { "toggle_suggestions", "Suggestions" },
            { "toggle_auto_indent", "Auto-indent" },
            { "log_max_char_reached", "Reached maximum InputField character length! ({0})" },

            // FreeCamPanel.cs
            { "panel_name_freecam", "Freecam" },
            { "button_freecam", "Freecam" },
            { "toggle_use_game_camera", "Use Game Camera?" },
            { "label_position", "Position" },
            { "input_freecam_pos_placeholder", "eg. 0 0 0" },
            { "label_movespeed", "MoveSpeed" },
            { "label_move_speed_colon", "Move Speed:" },
            { "input_movespeed_placeholder", "Default: 1" },
            { "text_freecam_controls", "Controls:\n- WASD / Arrows: Movement\n- Space / PgUp: Move up\n- LeftCtrl / PgDown: Move down\n- Right MouseButton: Free look\n- Shift: Super speed" },
            { "button_inspect_free_camera", "Inspect Free Camera" },
            { "button_end_freecam", "End Freecam" },
            { "button_begin_freecam", "Begin Freecam" },
            { "log_no_previous_camera", "There is no previous Camera found, reverting to default Free Cam." },
            { "log_could_not_parse_position", "Could not parse position to Vector3: {0}" },
            { "log_could_not_parse_value", "Could not parse value: {0}" },

            // HookManagerPanel.cs
            { "panel_name_hooks", "Hooks" },

            // InspectorPanel.cs
            { "panel_name_inspector", "Inspector" },
            { "dropdown_mouse_inspect", "Mouse Inspect" },
            { "dropdown_option_world", "World" },
            { "dropdown_option_ui", "UI" },
            { "button_close_all", "Close All" },

            // LogPanel.cs
            { "panel_name_log", "Log" },
            { "button_clear", "Clear" },
            { "button_open_log_file", "Open Log File" },
            { "toggle_log_unity_debug", "Log Unity Debug?" },

            // MouseInspectorResultsPanel.cs
            { "panel_name_ui_inspector_results", "UI Inspector Results" },
            { "text_format_ui_inspector_result", "<color=cyan>{0}</color> ({1})" },

            // ObjectExplorerPanel.cs
            { "panel_name_object_explorer", "Object Explorer" },
            { "tab_scene_explorer", "Scene Explorer" },
            { "tab_object_search", "Object Search" },

            // OptionsPanel.cs
            { "panel_name_options", "Options" },
            { "button_save_options", "Save Options" },

            // UIManager.cs
            { "text_format_main_title", "UE <i><color=grey>{0}</color></i>" },

            // Widgets/EvaluateWidget.cs
            { "label_generic_arguments", "Generic Arguments" },
            { "label_arguments", "Arguments" },
            { "button_evaluate", "Evaluate" },

            // Widgets/GameObjects/GameObjectInfoPanel.cs
            { "text_format_gameobject_tab", "[G] {0}" },
            { "button_view_parent", "◄ View Parent" },
            { "text_no_parent", "No parent" },
            { "text_none_asset_resource", "None (Asset/Resource)" },
            { "log_could_not_find_gameobject", "Could not find any GameObject name or path '{0}'!" },
            { "log_exception_setting_tag", "Exception setting tag! {0}" },
            { "log_exception_setting_hideflags", "Exception setting hideFlags: {0}" },
            { "button_copy_to_clipboard", "Copy to Clipboard" },
            { "label_activeself", "ActiveSelf" },
            { "label_isstatic", "IsStatic" },
            { "label_instance_id", "Instance ID:" },
            { "label_tag", "Tag:" },
            { "button_instantiate", "Instantiate" },
            { "button_destroy", "Destroy" },
            { "button_show_in_explorer", "Show in Explorer" },
            { "label_scene", "Scene:" },
            { "label_layer", "Layer:" },
            { "label_flags", "Flags:" }
        };
    }
}
