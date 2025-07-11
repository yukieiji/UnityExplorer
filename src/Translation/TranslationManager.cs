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
        { "clipboard", "Clipboard" },
        { "copied", "Copied!" },
        { "pasted", "Pasted!" },
        { "cannot_inspect", "Cannot inspect a null or destroyed object!" },
        { "cannot_assign", "Cannot assign '{0}' to '{1}'!" },
        { "current_paste", "Current paste:" },
        { "clear_clipboard", "Clear Clipboard" },
        { "not_set", "not set" },
        { "inspect", "Inspect" },
        { "not_set", "not set" },
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
        { "button_freecam", "Freecam" }, // Assuming this is the main button, might need different keys for "End Freecam" / "Begin Freecam" if they are separate UI elements
        { "toggle_use_game_camera", "Use Game Camera?" },
        { "label_position", "Position" },
        { "input_freecam_pos_placeholder", "eg. 0 0 0" },
        // "Reset" is already defined as "button_reset", consider if a more specific key is needed e.g., "button_reset_freecam_pos"
        { "label_movespeed", "MoveSpeed" }, // Potentially "label_move_speed"
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
        { "dropdown_mouse_inspect", "Mouse Inspect" }, // Default text for dropdown
        { "dropdown_option_world", "World" },
        { "dropdown_option_ui", "UI" }, // Note: "UI" is very generic, might already exist or need context if used elsewhere
        { "button_close_all", "Close All" },

        // LogPanel.cs
        { "panel_name_log", "Log" },
        { "button_clear", "Clear" }, // "Clear" is generic, "button_clear_log" might be better
        { "button_open_log_file", "Open Log File" },
        { "toggle_log_unity_debug", "Log Unity Debug?" },
        // "{filename}.txt" etc. are usually not translated.

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
        // Assuming "UE <i><color=grey>{0}</color></i>" is for the main title
        { "text_format_main_title", "UE <i><color=grey>{0}</color></i>" }
        // GameObject names like "MainNavbar", "Title" are usually not translated.
        // Button text for close might be covered by ConfigManager.Master_Toggle.Value.ToString() dynamically, or needs a specific key if static.

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
        { "button_copy_to_clipboard", "Copy to Clipboard" }, // Consider a more specific key if "Copy" is used elsewhere for different things
        { "label_activeself", "ActiveSelf" },
        { "label_isstatic", "IsStatic" },
        { "label_instance_id", "Instance ID:" },
        { "label_tag", "Tag:" },
        { "button_instantiate", "Instantiate" },
        { "button_destroy", "Destroy" }, // Generic "Destroy", consider "button_destroy_gameobject"
        { "button_show_in_explorer", "Show in Explorer" },
        { "label_scene", "Scene:" },
        { "label_layer", "Layer:" },
        { "label_flags", "Flags:" }
    };

    private static Dictionary<string, string> jp = new()
    {
        { "translation", "UnityExplorerの言語" },
        { "translation_hint", "UnityExplorerのメニューや機能の言語。このオプションを変更した場合、再起動が必要" },
        { "ue_toggle", "UnityExplorer Toggle" },
        { "ue_toggle_hint", "UnityExplorerのメニューと機能の有効/無効を切り替えるキー。" },
        { "hide_on_startup", "起動時に非表示" },
        { "hide_on_startup_hint", "起動時にUnityExplorerを非表示にしますか？" },
        { "startup_delay_time", "起動遅延時間" },
        { "startup_delay_time_hint", "UIが作成されるまでの起動時の遅延時間。" },
        { "target_display", "ターゲットディスプレイ" },
        { "target_display_hint", "UnityExplorerが使用するモニターのインデックス（複数ある場合）。0がデフォルトディスプレイ、1がセカンダリなど。この設定を変更する際は再起動を推奨します。追加モニターがプライマリモニターと同じ解像度であることを確認してください。" },
        { "force_unlock_mouse", "マウスの強制アンロック" },
        { "force_unlock_mouse_hint", "UnityExplorerメニューが開いているときに、カーソルを強制的にアンロック（表示）します。" },
        { "force_unlock_toggle_key", "強制アンロック切り替えキー" },
        { "force_unlock_toggle_key_hint", "'マウスの強制アンロック'設定を切り替えるキーバインド。UnityExplorerが開いているときのみ使用可能です。" },
        { "disable_eventsystem_override", "EventSystemオーバーライドの無効化" },
        { "disable_eventsystem_override_hint", "有効にすると、UnityExplorerはゲームのEventSystemをオーバーライドしません。\n<b>有効にするには再起動が必要な場合があります。</b>" },
        { "default_output_path", "デフォルト出力パス" },
        { "default_output_path_hint", "UnityExplorerからエクスポートする際のデフォルト出力パス。" },
        { "dnspy_path", "dnSpyパス" },
        { "dnspy_path_hint", "dnSpy.exe（64ビット）へのフルパス。" },
        { "main_navbar_anchor", "メインナビゲーションバーアンカー" },
        { "main_navbar_anchor_hint", "UnityExplorerメインナビゲーションバーの垂直アンカー。移動したい場合に使用します。" },
        { "log_unity_debug", "Unityデバッグログ" },
        { "log_unity_debug_hint", "UnityEngine.Debug.LogメッセージをUnityExplorerのログに出力しますか？" },
        { "log_to_disk", "ディスクへのログ保存" },
        { "log_to_disk_hint", "UnityExplorerがログファイルをディスクに保存しますか？" },
        { "world_mouse_inspect_keybind", "ワールドマウス調査キーバインド" },
        { "world_mouse_inspect_keybind_hint", "ワールドモードのマウス調査を開始するためのオプションのキーバインド。" },
        { "ui_mouse_inspect_keybind", "UIマウス調査キーバインド" },
        { "ui_mouse_inspect_keybind_hint", "UIモードのマウス調査を開始するためのオプションのキーバインド。" },
        { "csharp_console_assembly_blacklist", "C#コンソールアセンブリブラックリスト" },
        { "csharp_console_assembly_blacklist_hint", "C#コンソールによって参照されるアセンブリ名をブラックリストに登録するために使用します。C#コンソールのリセットが必要です。\n各アセンブリをセミコロン「;」で区切ります。\n例えば、Assembly-CSharpをブラックリストに登録するには、「Assembly-CSharp;」と追加します。" },
        { "member_signature_blacklist", "メンバー署名ブラックリスト" },
        { "member_signature_blacklist_hint", "クラッシュやその他の問題を引き起こすことがわかっている特定のメンバー署名をブラックリストに登録するために使用します。\r\n署名をセミコロン「;」で区切ります。\r\n例えば、Camera.mainをブラックリストに登録するには、「UnityEngine.Camera.main;」と追加します。" },
        { "clipboard", "クリップボード" },
        { "copied", "コピーしました！" },
        { "pasted", "貼り付けました！" },
        { "cannot_inspect", "nullまたは破棄されたオブジェクトを調査できません！" },
        { "cannot_assign", "'{0}'を'{1}'に割り当てることはできません！" },
        { "current_paste", "現在の貼り付け：" },
        { "clear_clipboard", "クリップボードをクリア" },
        { "not_set", "未設定" },
        { "inspect", "調査" },
        { "autocompleter", "自動補完" },
        { "help_updown_esc", "上下で選択、Enterで使用、Escで閉じる" },

        // CSConsolePanel.cs
        { "panel_name_csharp_console", "C# コンソール" },
        { "button_compile", "コンパイル" },
        { "button_reset", "リセット" }, // FreeCamPanelのResetボタンと共通の可能性あり
        { "dropdown_help", "ヘルプ" },
        { "toggle_compile_on_ctrl_r", "Ctrl+Rでコンパイル" },
        { "toggle_suggestions", "入力候補" },
        { "toggle_auto_indent", "自動インデント" },
        { "log_max_char_reached", "最大入力文字数に達しました！ ({0})" },

        // FreeCamPanel.cs
        { "panel_name_freecam", "フリーカメラ" },
        { "button_freecam", "フリーカメラ" },
        { "toggle_use_game_camera", "ゲームカメラを使用" },
        { "label_position", "位置" },
        { "input_freecam_pos_placeholder", "例: 0 0 0" },
        { "label_movespeed", "移動速度" }, // label_move_speed の方が適切だったかもしれません
        { "label_move_speed_colon", "移動速度:" },
        { "input_movespeed_placeholder", "デフォルト: 1" },
        { "text_freecam_controls", "操作:\n- WASD / 矢印キー: 移動\n- Space / PgUp: 上昇\n- 左Ctrl / PgDown: 下降\n- 右マウスボタン: カメラ操作\n- Shift: 高速移動" },
        { "button_inspect_free_camera", "フリーカメラを調査" },
        { "button_end_freecam", "フリーカメラ終了" },
        { "button_begin_freecam", "フリーカメラ開始" },
        { "log_no_previous_camera", "以前のカメラが見つからないため、デフォルトのフリーカメラに戻します。" },
        { "log_could_not_parse_position", "位置をVector3に解析できませんでした: {0}" },
        { "log_could_not_parse_value", "値を解析できませんでした: {0}" },

        // HookManagerPanel.cs
        { "panel_name_hooks", "フック" },

        // InspectorPanel.cs
        { "panel_name_inspector", "インスペクター" },
        { "dropdown_mouse_inspect", "マウス調査" },
        { "dropdown_option_world", "ワールド" },
        { "dropdown_option_ui", "UI" },
        { "button_close_all", "すべて閉じる" },

        // LogPanel.cs
        { "panel_name_log", "ログ" },
        { "button_clear", "クリア" }, // "button_clear_log" の方が適切だったかもしれません
        { "button_open_log_file", "ログファイルを開く" },
        { "toggle_log_unity_debug", "Unityデバッグをログに記録しますか？" },

        // MouseInspectorResultsPanel.cs
        { "panel_name_ui_inspector_results", "UIインスペクター結果" },
        { "text_format_ui_inspector_result", "<color=cyan>{0}</color> ({1})" }, // 通常、書式設定文字列内のテキストは翻訳しませんが、必要であれば検討

        // ObjectExplorerPanel.cs
        { "panel_name_object_explorer", "オブジェクトエクスプローラー" },
        { "tab_scene_explorer", "シーンエクスプローラー" },
        { "tab_object_search", "オブジェクト検索" },

        // OptionsPanel.cs
        { "panel_name_options", "オプション" },
        { "button_save_options", "オプションを保存" },

        // UIManager.cs
        { "text_format_main_title", "UE <i><color=grey>{0}</color></i>" }, // バージョン表示なので、基本的には翻訳不要か、UE のみ翻訳

        // Widgets/EvaluateWidget.cs
        { "label_generic_arguments", "総称引数" },
        { "label_arguments", "引数" },
        { "button_evaluate", "評価" },

        // Widgets/GameObjects/GameObjectInfoPanel.cs
        { "text_format_gameobject_tab", "[G] {0}" }, // {0} はGameObject名
        { "button_view_parent", "◄ 親を表示" },
        { "text_no_parent", "親なし" },
        { "text_none_asset_resource", "なし (アセット/リソース)" },
        { "log_could_not_find_gameobject", "GameObject名またはパス '{0}' が見つかりませんでした！" },
        { "log_exception_setting_tag", "タグ設定中の例外！ {0}" },
        { "log_exception_setting_hideflags", "hideFlags設定中の例外: {0}" },
        { "button_copy_to_clipboard", "クリップボードにコピー" },
        { "label_activeself", "アクティブセルフ" }, // または "自身がアクティブ"
        { "label_isstatic", "静的" }, // または "isStatic" のまま
        { "label_instance_id", "インスタンスID:" },
        { "label_tag", "タグ:" },
        { "button_instantiate", "インスタンス化" },
        { "button_destroy", "破棄" }, // または "button_destroy_gameobject" -> "ゲームオブジェクトを破棄"
        { "button_show_in_explorer", "エクスプローラーに表示" },
        { "label_scene", "シーン:" },
        { "label_layer", "レイヤー:" },
        { "label_flags", "フラグ:" }
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

    public static string Get(string key, params object[] parts)
    {
        string row_val = Get(key);
        return string.Format(row_val, parts);
    }

    public static string Get(string key)
        => Get(ConfigManager.Lang_Toggle.Value, key);
}
