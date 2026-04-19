using UnityEngine.SceneManagement;

#nullable enable

namespace UnityExplorer.ObjectExplorer;

public static class SceneHandler
{
    /// <summary>The currently inspected Scene.</summary>
    public static Scene? SelectedScene
    {
        get => selectedScene;
        internal set
        {
            if (selectedScene.HasValue && selectedScene == value)
            {
                return;
            }
            if (!value.HasValue)
            {
                return;
            }
            selectedScene = value;
            OnInspectedSceneChanged?.Invoke(selectedScene.Value);
        }
    }
    private static Scene? selectedScene;

    /// <summary>The GameObjects in the currently inspected scene.</summary>
    public static IEnumerable<GameObject> CurrentRootObjects { get; private set; } = new GameObject[0];

    /// <summary>All currently loaded Scenes.</summary>
    public static List<Scene> LoadedScenes { get; private set; } = new();
    //private static HashSet<Scene> previousLoadedScenes;

    /// <summary>The names of all scenes in the build settings, if they could be retrieved.</summary>
    public static List<string> AllSceneNames { get; private set; } = new();

    /// <summary>Invoked when the currently inspected Scene changes. The argument is the new scene.</summary>
    public static event Action<Scene>? OnInspectedSceneChanged;

    /// <summary>Invoked whenever the list of currently loaded Scenes changes. The argument contains all loaded scenes after the change.</summary>
    public static event Action<List<Scene>>? OnLoadedScenesUpdated;

    /// <summary>Generally will be 2, unless DontDestroyExists == false, then this will be 1.</summary>
    internal static int DefaultSceneCount => 1 + (DontDestroyExists ? 1 : 0);

    /// <summary>Whether or not we are currently inspecting the "HideAndDontSave" asset scene.</summary>
    public static bool InspectingAssetScene => SelectedScene.HasValue && GetSceneHandleInt(SelectedScene.Value) == -1;

    /// <summary>Whether or not we successfuly retrieved the names of the scenes in the build settings.</summary>
    public static bool WasAbleToGetScenesInBuild { get; private set; }

    /// <summary>Whether or not the "DontDestroyOnLoad" scene exists in this game.</summary>
    public static bool DontDestroyExists { get; private set; }

    private const string dontDestroyName = "DontDestroyOnLoad";

    // Cached reflection fields for Unity 6 compatibility:
    // Scene.m_Handle changed from int to SceneHandle { EntityId m_Value { int m_Data } }
    private static FieldInfo? s_SceneM_Handle;
    private static FieldInfo? s_SceneHandleM_Value;
    private static FieldInfo? s_EntityIdM_Data;
    private static bool s_HandleFieldsCached;

    private static void EnsureHandleFieldsCached()
    {
        if (s_HandleFieldsCached) return;
        s_HandleFieldsCached = true;
        s_SceneM_Handle = typeof(Scene).GetField("m_Handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (s_SceneM_Handle != null && s_SceneM_Handle.FieldType != typeof(int))
        {
            s_SceneHandleM_Value = s_SceneM_Handle.FieldType.GetField("m_Value", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (s_SceneHandleM_Value != null)
                s_EntityIdM_Data = s_SceneHandleM_Value.FieldType.GetField("m_Data", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }

    // Returns the int handle value from a Scene, compatible with both old Unity (int m_Handle)
    // and Unity 6+ (SceneHandle { EntityId { int m_Data } }).
    internal static int GetSceneHandleInt(Scene scene)
    {
        EnsureHandleFieldsCached();
        if (s_SceneM_Handle == null) return 0;
        object? val = s_SceneM_Handle.GetValue(scene);
        if (val is int intVal) return intVal;
        if (val == null || s_SceneHandleM_Value == null || s_EntityIdM_Data == null) return 0;
        object? entityId = s_SceneHandleM_Value.GetValue(val);
        if (entityId == null) return 0;
        return (int)(s_EntityIdM_Data.GetValue(entityId) ?? 0);
    }

    // Creates a Scene struct with the given int handle value via reflection, compatible with
    // both old Unity (int m_Handle) and Unity 6+ (SceneHandle { EntityId { int m_Data } }).
    private static Scene CreateSceneByHandle(int handleValue)
    {
        EnsureHandleFieldsCached();
        object boxedScene = new Scene();
        if (s_SceneM_Handle == null) return default;
        if (s_SceneM_Handle.FieldType == typeof(int))
        {
            s_SceneM_Handle.SetValue(boxedScene, handleValue);
        }
        else if (s_SceneHandleM_Value != null && s_EntityIdM_Data != null)
        {
            object boxedEntityId = Activator.CreateInstance(s_SceneHandleM_Value.FieldType)!;
            s_EntityIdM_Data.SetValue(boxedEntityId, handleValue);
            object boxedHandle = Activator.CreateInstance(s_SceneM_Handle.FieldType)!;
            s_SceneHandleM_Value.SetValue(boxedHandle, boxedEntityId);
            s_SceneM_Handle.SetValue(boxedScene, boxedHandle);
        }
        return (Scene)boxedScene;
    }

    internal static void Init()
    {
        // Check if the game has "DontDestroyOnLoad"
        try
        {
            Type? sceneType = ReflectionUtility.GetTypeByName("UnityEngine.SceneManagement.Scene");
            if (sceneType == null)
            {
                throw new Exception("This version of Unity does not ship with the 'Scene' class, or it was not unstripped.");
            }
            MethodInfo? method = sceneType.GetMethod("GetNameInternal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                throw new Exception("Could not find Scene.GetNameInternal.");
            }

            // Unity 6+ changed the parameter from int to SceneHandle struct — detect and construct appropriately.
            object handleArg;
            ParameterInfo[] methodParams = method.GetParameters();
            if (methodParams.Length == 1 && methodParams[0].ParameterType == typeof(int))
            {
                handleArg = -12;
            }
            else if (methodParams.Length == 1)
            {
                // Unity 6+: build the SceneHandle using the same nested-struct helper we cache for Update()
                EnsureHandleFieldsCached();
                if (s_SceneM_Handle != null)
                {
                    // Extract the SceneHandle from a synthetic Scene built with handle = -12
                    object boxedScene = CreateSceneByHandle(-12);
                    handleArg = s_SceneM_Handle.GetValue(boxedScene)!;
                }
                else
                {
                    throw new Exception("Scene.GetNameInternal has an unexpected signature and SceneHandle fields could not be resolved.");
                }
            }
            else
            {
                throw new Exception($"Scene.GetNameInternal has unexpected signature: {methodParams.Length} parameters.");
            }

            string? sceneName = (string?)method.Invoke(null, [handleArg]);
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new Exception("Scene.GetNameInternal returned null for DontDestroyOnLoad scene.");
            }
            DontDestroyExists = sceneName == dontDestroyName;
        }
        catch (Exception ex)
        {
            ExplorerCore.LogWarning($"Unable to check for existence of DontDestroyOnLoad scene via Scene.GetNameInternal: {ex}");
            try
            {
#pragma warning disable CS0618 // 型またはメンバーが旧型式です
                ExplorerCore.LogWarning("Falling back to checking loaded scenes for DontDestroyOnLoad via SceneManager.GetAllScenes(). This uses a deprecated API.");
                DontDestroyExists = SceneManager.GetAllScenes().Any(s => s.name == dontDestroyName);
#pragma warning restore CS0618 // 型またはメンバーが旧型式です
            }
            catch (Exception fallbackEx)
            {
                ExplorerCore.LogWarning($"SceneManager.GetAllScenes() fallback also failed ({fallbackEx.Message}). Defaulting DontDestroyExists to true.");
                DontDestroyExists = true;
            }
        }

        // Try to get all scenes in the build settings. This may not work.
        try
        {
            Type sceneUtil = ReflectionUtility.GetTypeByName("UnityEngine.SceneManagement.SceneUtility");
            if (sceneUtil == null)
            {
                throw new Exception("This version of Unity does not ship with the 'SceneUtility' class, or it was not unstripped.");
            }

            MethodInfo? method = sceneUtil.GetMethod("GetScenePathByBuildIndex", ReflectionUtility.FLAGS);
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                string? scenePath = (string?)method?.Invoke(null, [ i ]);
                if (string.IsNullOrEmpty(scenePath))
                {
                    continue;
                }
                AllSceneNames.Add(scenePath!);
            }

            WasAbleToGetScenesInBuild = true;
        }
        catch (Exception ex)
        {
            WasAbleToGetScenesInBuild = false;
            ExplorerCore.LogWarning($"Unable to generate list of all Scenes in the build: {ex}");
        }
    }

    internal static void Update()
    {
        // Inspected scene will exist if it's DontDestroyOnLoad or HideAndDontSave
        bool inspectedExists =
            SelectedScene.HasValue
            && ((DontDestroyExists && GetSceneHandleInt(SelectedScene.Value) == -12)
                || GetSceneHandleInt(SelectedScene.Value) == -1);

        LoadedScenes.Clear();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene == default || !scene.isLoaded || !scene.IsValid())
            {
                continue;
            }

            // If we have not yet confirmed inspectedExists, check if this scene is our currently inspected one.
            if (!inspectedExists && scene == SelectedScene)
            {
                inspectedExists = true;
            }

            LoadedScenes.Add(scene);
        }

        if (DontDestroyExists)
        {
            LoadedScenes.Add(CreateSceneByHandle(-12));
        }
        LoadedScenes.Add(CreateSceneByHandle(-1));

        // Default to first scene if none selected or previous selection no longer exists.
        if (!inspectedExists)
        {
            SelectedScene = LoadedScenes.First();
        }

        // Notify on the list changing at all
        OnLoadedScenesUpdated?.Invoke(LoadedScenes);

        // Finally, update the root objects list.
        if (SelectedScene.HasValue && SelectedScene.Value.IsValid())
        {
            CurrentRootObjects = RuntimeHelper.GetRootGameObjects(SelectedScene.Value);
        }
        else
        {
            UnityEngine.Object[] allObjects = RuntimeHelper.FindObjectsOfTypeAll(typeof(GameObject));
            List<GameObject> objects = new();
            foreach (UnityEngine.Object obj in allObjects)
            {
                GameObject? go = obj.TryCast<GameObject>();
                if (go != null &&
                    go.transform != null &&
                    go.transform.parent == null && 
                    !go.scene.IsValid())
                {
                    objects.Add(go);
                }
            }
            CurrentRootObjects = objects;
        }
    }
}
