using UnityExplorer.UI;

namespace UnityExplorer.Config
{
    public static class ConfigManager
    {
        internal static readonly Dictionary<string, IConfigElement> ConfigElements = new();
        internal static readonly Dictionary<string, IConfigElement> InternalConfigs = new();

        // Each Mod Loader has its own ConfigHandler.
        // See the UnityExplorer.Loader namespace for the implementations.
        public static ConfigHandler Handler { get; private set; }

        // Actual UE Settings
        public static ConfigElement<TranslationManager.Lang> Lang_Toggle;
        public static ConfigElement<KeyCode> Master_Toggle;
        public static ConfigElement<bool> Hide_On_Startup;
        public static ConfigElement<float> Startup_Delay_Time;
        public static ConfigElement<bool> Disable_EventSystem_Override;
        public static ConfigElement<int> Target_Display;
        public static ConfigElement<bool> Force_Unlock_Mouse;
        public static ConfigElement<KeyCode> Force_Unlock_Toggle;
        public static ConfigElement<string> Default_Output_Path;
        public static ConfigElement<string> DnSpy_Path;
        public static ConfigElement<bool> Log_Unity_Debug;
        public static ConfigElement<bool> Log_To_Disk;
        public static ConfigElement<UIManager.VerticalAnchor> Main_Navbar_Anchor;
        public static ConfigElement<KeyCode> World_MouseInspect_Keybind;
        public static ConfigElement<KeyCode> UI_MouseInspect_Keybind;
        public static ConfigElement<string> CSConsole_Assembly_Blacklist;
        public static ConfigElement<string> Reflection_Signature_Blacklist;

        // internal configs
        internal static InternalConfigHandler InternalHandler { get; private set; }
        internal static readonly Dictionary<UIManager.Panels, ConfigElement<string>> PanelSaveData = new();

        internal static ConfigElement<string> GetPanelSaveData(UIManager.Panels panel)
        {
            if (!PanelSaveData.ContainsKey(panel))
                PanelSaveData.Add(panel, new ConfigElement<string>(panel.ToString(), string.Empty, true));
            return PanelSaveData[panel];
        }

        public static void Init(ConfigHandler configHandler)
        {
            Handler = configHandler;
            Handler.Init();

            InternalHandler = new InternalConfigHandler();
            InternalHandler.Init();

            CreateConfigElements();

            Handler.LoadConfig();
            InternalHandler.LoadConfig();

#if STANDALONE
            if (Loader.Standalone.ExplorerEditorBehaviour.Instance)
                Loader.Standalone.ExplorerEditorBehaviour.Instance.LoadConfigs();
#endif
        }

        internal static void RegisterConfigElement<T>(ConfigElement<T> configElement)
        {
            if (!configElement.IsInternal)
            {
                Handler.RegisterConfigElement(configElement);
                ConfigElements.Add(configElement.NameKey, configElement);
            }
            else
            {
                InternalHandler.RegisterConfigElement(configElement);
                InternalConfigs.Add(configElement.NameKey, configElement);
            }
        }

        private static void CreateConfigElements()
        {
            Lang_Toggle = new("translation", TranslationManager.Lang.English);

            Master_Toggle = new("ue_toggle", KeyCode.F7);

            Hide_On_Startup = new("hide_on_startup", false);

            Startup_Delay_Time = new("startup_delay_time", 1f);

            Target_Display = new("target_display", 0);

            Force_Unlock_Mouse = new("force_unlock_mouse", true);
            Force_Unlock_Mouse.OnValueChanged += (bool value) => UniverseLib.Config.ConfigManager.Force_Unlock_Mouse = value;

            Force_Unlock_Toggle = new("force_unlock_toggle_key", KeyCode.None);

            Disable_EventSystem_Override = new("disable_eventsystem_override", false);
            Disable_EventSystem_Override.OnValueChanged += (bool value) => UniverseLib.Config.ConfigManager.Disable_EventSystem_Override = value;

            Default_Output_Path = new("default_output_path",
                Path.Combine(ExplorerCore.ExplorerFolder, "Output"));

            DnSpy_Path = new("dnspy_path",  @"C:/Program Files/dnspy/dnSpy.exe");

            Main_Navbar_Anchor = new("main_navbar_anchor", UIManager.VerticalAnchor.Top);

            Log_Unity_Debug = new("log_unity_debug", false);

            Log_To_Disk = new("log_to_disk", true);

            World_MouseInspect_Keybind = new("world_mouse_inspect_keybind", KeyCode.None);

            UI_MouseInspect_Keybind = new("ui_mouse_inspect_keybind", KeyCode.None);

            CSConsole_Assembly_Blacklist = new("csharp_console_assembly_blacklist", "");

            Reflection_Signature_Blacklist = new("member_signature_blacklist", "");
        }
    }
}
