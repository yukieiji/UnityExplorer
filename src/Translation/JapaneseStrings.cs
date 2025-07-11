
namespace UnityExplorer.Translation;

public static class JapaneseStrings
{
    public static Dictionary<string, string> Data => new Dictionary<string, string>()
    {
        { "translation", "UnityExplorerの言語" },
        { "translation_hint", "UnityExplorerのメニューや機能の言語。このオプションを変更した場合、再起動が必要" },
        { "ue_toggle", "UnityExplorer Toggle" },
        { "ue_toggle_hint", "UnityExplorerのメニューと機能の有効/無効を切り替えるキー" },
        { "hide_on_startup", "起動時に非表示" },
        { "hide_on_startup_hint", "起動時にUnityExplorerを非表示にするか" },
        { "startup_delay_time", "起動遅延時間" },
        { "startup_delay_time_hint", "UIが作成されるまでの起動時の遅延時間" },
        { "target_display", "表示ディスプレイ" },
        { "target_display_hint", "UnityExplorerが使用するモニター。0がメインディスプレイ、1がセカンダリなど。この設定を変更する際は再起動を推奨します。追加モニターがプライマリモニターと同じ解像度であることを確認してください。" },
        { "force_unlock_mouse", "マウスの強制アンロック" },
        { "force_unlock_mouse_hint", "UnityExplorerメニューが開いているときに、カーソルを強制的に表示します。" },
        { "force_unlock_toggle_key", "マウスカーソルを強制的に表示するキー" },
        { "force_unlock_toggle_key_hint", "'マウスカーソルを強制的に表示するキー'の設定。UnityExplorerが開いているときのみ使用可能です。" },
        { "disable_eventsystem_override", "イベントシステムの上書き無効化" },
        { "disable_eventsystem_override_hint", "有効にすると、UnityExplorerはゲームのイベントシステムをオーバーライドしません。\n<b>有効にするには再起動が必要な場合があります。</b>" },
        { "default_output_path", "出力パス" },
        { "default_output_path_hint", "UnityExplorerからエクスポートする際の出力パス。" },
        { "dnspy_path", "dnSpyパス" },
        { "dnspy_path_hint", "dnSpy.exe（64ビット）へのフルパス。" },
        { "main_navbar_anchor", "メインナビゲーションバーアンカー" },
        { "main_navbar_anchor_hint", "UnityExplorerメインナビゲーションバーの垂直アンカー。移動したい場合に使用します。" },
        { "log_unity_debug", "Unityデバッグログ" },
        { "log_unity_debug_hint", "UnityEngine.Debug.LogメッセージをUnityExplorerのログに出力するか" },
        { "log_to_disk", "ログ保存" },
        { "log_to_disk_hint", "UnityExplorerがログを保存するか" },
        { "world_mouse_inspect_keybind", "ワールドマウス調査キーバインド" },
        { "world_mouse_inspect_keybind_hint", "ワールドモードのマウス調査を開始するためのオプションのキーバインド。" },
        { "ui_mouse_inspect_keybind", "マウスインスペクターキー" },
        { "ui_mouse_inspect_keybind_hint", "マウスインスペクターを開始するためのキーバインド" },
        { "csharp_console_assembly_blacklist", "アセンブリブラックリスト" },
        { "csharp_console_assembly_blacklist_hint", "C# コンソール使用時に使用しないアセンブリのリスト。C#　コンソールのリセットが必要です。\n各アセンブリをセミコロン「;」で区切ります。\n例えば、Assembly-CSharpをブラックリストに登録するには、「Assembly-CSharp;」と追加します。" },
        { "member_signature_blacklist", "メンバーブラックリスト" },
        { "member_signature_blacklist_hint", "クラッシュやその他の問題を引き起こすことがわかっている特定のクラスメンバーをブラックリストに登録するために使用します。\r\n署名をセミコロン「;」で区切ります。\r\n例えば、Camera.mainをブラックリストに登録するには、「UnityEngine.Camera.main;」と追加します。" },
        { "clipboard", "クリップボード" },
        { "copied", "コピーしました！" },
        { "pasted", "貼り付けました！" },
        { "cannot_inspect", "nullまたは削除されたオブジェクトをインスペクトできません！" },
        { "cannot_assign", "'{0}'を'{1}'に割り当てることはできません！" },
        { "current_paste", "現在の貼り付け：" },
        { "clear_clipboard", "クリップボードをクリア" },
        { "not_set", "未設定" },
        { "inspect", "インスペクト" },
        { "autocompleter", "自動補完" },
        { "help_updown_esc", "上下で選択、Enterで使用、Escで閉じる" },

        // CSConsolePanel.cs
        { "panel_name_csharp_console", "C# コンソール" },
        { "button_compile", "コンパイル" },
        { "button_reset", "リセット" },
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
        { "label_movespeed", "移動速度" },
        { "label_move_speed_colon", "移動速度:" },
        { "input_movespeed_placeholder", "デフォルト: 1" },
        { "text_freecam_controls", "操作:\n- WASD / 矢印キー: 移動\n- Space / PgUp: 上昇\n- 左Ctrl / PgDown: 下降\n- 右マウスボタン: カメラ操作\n- Shift: 高速移動" },
        { "button_inspect_free_camera", "フリーカメラを調査" },
        { "button_end_freecam", "フリーカメラ終了" },
        { "button_begin_freecam", "フリーカメラ開始" },
        { "log_no_previous_camera", "カメラが見つからないため、デフォルトのフリーカメラに戻します。" },
        { "log_could_not_parse_position", "位置をVector3にパースできませんでした: {0}" },
        { "log_could_not_parse_value", "値をパースできませんでした: {0}" },

        // HookManagerPanel.cs
        { "panel_name_hooks", "フック" },

        // InspectorPanel.cs
        { "panel_name_inspector", "インスペクター" },
        { "dropdown_mouse_inspect", "マウスインスペクター" },
        { "dropdown_option_world", "ワールド" },
        { "dropdown_option_ui", "UI" },
        { "button_close_all", "すべて閉じる" },

        // LogPanel.cs
        { "panel_name_log", "ログ" },
        { "button_clear", "クリア" },
        { "button_open_log_file", "ログファイルを開く" },
        { "toggle_log_unity_debug", "Unityデバッグをログに記録しますか？" },

        // MouseInspectorResultsPanel.cs
        { "panel_name_ui_inspector_results", "結果" },
        { "text_format_ui_inspector_result", "<color=cyan>{0}</color> ({1})" },

        // ObjectExplorerPanel.cs
        { "panel_name_object_explorer", "オブジェクトエクスプローラー" },
        { "tab_scene_explorer", "シーンエクスプローラー" },
        { "tab_object_search", "検索" },

        // OptionsPanel.cs
        { "panel_name_options", "オプション" },
        { "button_save_options", "保存" },

        // UIManager.cs
        { "text_format_main_title", "UE <i><color=grey>{0}</color></i>" },

        // Widgets/EvaluateWidget.cs
        { "label_generic_arguments", "ジェネリック引数" },
        { "label_arguments", "引数" },
        { "button_evaluate", "実行" },

        // Widgets/GameObjects/GameObjectInfoPanel.cs
        { "text_format_gameobject_tab", "[G] {0}" },
        { "button_view_parent", "◄ 親を表示" },
        { "text_no_parent", "親なし" },
        { "text_none_asset_resource", "なし (アセット/リソース)" },
        { "log_could_not_find_gameobject", "GameObject名またはパス '{0}' が見つかりませんでした！" },
        { "log_exception_setting_tag", "タグ設定中の例外： {0}" },
        { "log_exception_setting_hideflags", "hideFlags設定中の例外: {0}" },
        { "button_copy_to_clipboard", "クリップボードにコピー" },
        { "label_activeself", "有効化" },
        { "label_isstatic", "静的" },
        { "label_instance_id", "インスタンスID:" },
        { "label_tag", "タグ:" },
        { "button_instantiate", "インスタンス化" },
        { "button_destroy", "削除" },
        { "button_show_in_explorer", "エクスプローラーに表示" },
        { "label_scene", "シーン:" },
        { "label_layer", "レイヤー:" },
        { "label_flags", "フラグ:" }
    };
}
