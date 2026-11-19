using UnityEngine.SceneManagement;
using UniverseLib;

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
    public static bool InspectingAssetScene => SelectedScene.HasValue && GetHandle(SelectedScene.Value) == AssetSceneHandle;

    /// <summary>Whether or not we successfuly retrieved the names of the scenes in the build settings.</summary>
    public static bool WasAbleToGetScenesInBuild { get; private set; }

    /// <summary>Whether or not the "DontDestroyOnLoad" scene exists in this game.</summary>
    public static bool DontDestroyExists { get; private set; }

    private const string dontDestroyName = "DontDestroyOnLoad";

    internal const int DontDestroyHandle = -12;
    internal const int AssetSceneHandle = -1;

    // ---------------------------------------------------------------------
    // Scene handle compatibility layer.
    //
    // Before Unity 6000.3, Scene.handle / Scene.m_Handle is an int.
    // From 6000.3 it is a SceneHandle struct { EntityId m_Value { int m_Data } }.
    // Compiled code that touches .handle / .m_Handle directly breaks on one of
    // the two layouts, so everything goes through reflection here instead.
    // ---------------------------------------------------------------------

    private const BindingFlags InstFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    private static readonly PropertyInfo? handleProp = typeof(Scene).GetProperty("handle", InstFlags);
    private static readonly FieldInfo? handleField =
        typeof(Scene).GetField("m_Handle", InstFlags)
        ?? typeof(Scene).GetFields(InstFlags).FirstOrDefault();

    private static Scene? cachedDontDestroyScene;
    private static Scene? cachedAssetScene;
    private static bool loggedUpdateError;

    /// <summary>Gets the scene's handle as an int, regardless of the Unity version's handle layout.</summary>
    internal static int GetHandle(Scene scene)
    {
        try
        {
            object? raw = handleProp != null
                ? handleProp.GetValue(scene)
                : handleField?.GetValue(scene);

            if (TryUnwrapInt(raw, out int value))
            {
                return value;
            }
        }
        catch
        {
            // ignored, fall through
        }
        return int.MinValue; // "unknown", never equals a sentinel
    }

    private static bool TryUnwrapInt(object? o, out int value, int depth = 0)
    {
        value = 0;
        switch (o)
        {
            case null: return false;
            case int i: value = i; return true;
            case uint ui: value = unchecked((int)ui); return true;
            case long l: value = unchecked((int)l); return true;
            case ulong ul: value = unchecked((int)ul); return true;
        }

        Type type = o.GetType();
        if (depth > 4 || !type.IsValueType || type.IsPrimitive || type.IsEnum)
        {
            return false;
        }

        foreach (FieldInfo field in type.GetFields(InstFlags))
        {
            if (TryUnwrapInt(field.GetValue(o), out value, depth + 1))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Returns a copy of a (boxed) value type with its first integer leaf set to <paramref name="value"/>.</summary>
    private static object? WithInt(Type type, object? existing, int value, int depth = 0)
    {
        if (type == typeof(int)) return value;
        if (type == typeof(uint)) return unchecked((uint)value);
        if (type == typeof(long)) return (long)value;
        if (type == typeof(ulong)) return unchecked((ulong)(long)value);

        if (depth > 4 || !type.IsValueType || type.IsPrimitive || type.IsEnum)
        {
            return null;
        }

        object boxed = existing ?? Activator.CreateInstance(type)!;
        foreach (FieldInfo field in type.GetFields(InstFlags))
        {
            object? inner = WithInt(field.FieldType, field.GetValue(boxed), value, depth + 1);
            if (inner != null)
            {
                field.SetValue(boxed, inner);
                return boxed;
            }
        }
        return null;
    }

    /// <summary>Builds a Scene struct carrying a sentinel handle (-12 for DontDestroyOnLoad, -1 for the asset scene).</summary>
    internal static Scene MakeScene(int handle)
    {
        try
        {
            if (handleField != null)
            {
                object boxed = new Scene();
                object? newValue = WithInt(handleField.FieldType, handleField.GetValue(boxed), handle);
                if (newValue != null)
                {
                    handleField.SetValue(boxed, newValue);
                    return (Scene)boxed;
                }
            }
        }
        catch (Exception ex)
        {
            ExplorerCore.LogWarning($"Unable to create sentinel Scene with handle {handle}: {ex.Message}");
        }
        return default;
    }

    private static Scene DontDestroyScene => cachedDontDestroyScene ??= MakeScene(DontDestroyHandle);
    private static Scene AssetScene => cachedAssetScene ??= MakeScene(AssetSceneHandle);

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
            ParameterInfo? param = method?.GetParameters().FirstOrDefault();
            if (method == null || param == null)
            {
                throw new Exception("Scene.GetNameInternal is missing in this Unity version.");
            }

            // int on older Unity, SceneHandle struct on Unity 6000.3+.
            object? arg = WithInt(param.ParameterType, null, DontDestroyHandle);
            if (arg == null)
            {
                throw new Exception($"Scene.GetNameInternal takes an unsupported parameter type: {param.ParameterType}");
            }

            string? sceneName = (string?)method.Invoke(null, [arg]);
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new Exception("Scene.GetNameInternal returned null for DontDestroyOnLoad scene.");
            }
            DontDestroyExists = sceneName == dontDestroyName;
        }
        catch (Exception ex)
        {
            ExplorerCore.LogWarning($"Unable to check for existence of DontDestroyOnLoad scene via Scene.GetNameInternal: {ex}");
#pragma warning disable CS0618 // 型またはメンバーが旧型式です
            ExplorerCore.LogWarning("Falling back to checking loaded scenes for DontDestroyOnLoad via SceneManager.GetAllScenes(). This uses a deprecated API.");
            // 非推奨APIだけど、6年近くたってもまだ使われてるので仕方なく使う
            try
            {
                DontDestroyExists = SceneManager.GetAllScenes().Any(s => s.name == dontDestroyName);
            }
            catch (Exception ex2)
            {
                // GetAllScenes can be stripped. Every Unity since 5.3 has a DontDestroyOnLoad scene.
                DontDestroyExists = true;
                ExplorerCore.LogWarning($"SceneManager.GetAllScenes() unavailable ({ex2.Message}); assuming DontDestroyOnLoad scene exists.");
            }
#pragma warning restore CS0618 // 型またはメンバーが旧型式です
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
                string? scenePath = (string?)method?.Invoke(null, [i]);
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
        // A failure here must never take the whole UI down with it.
        try
        {
            UpdateInternal();
        }
        catch (Exception ex)
        {
            if (!loggedUpdateError)
            {
                loggedUpdateError = true;
                ExplorerCore.LogWarning($"SceneHandler.Update failed (further errors suppressed): {ex}");
            }
        }
    }

    private static bool providerRootsBroken;

    /// <summary>
    /// UniverseLib's RuntimeHelper.GetRootGameObjects reads Scene.handle as an int, which throws
    /// MissingMethodException on Unity 6000.3+. Fall back to Scene.GetRootGameObjects() directly,
    /// which is unaffected by the handle change.
    /// </summary>
    private static IEnumerable<GameObject> GetRootObjects(Scene scene)
    {
        if (!providerRootsBroken)
        {
            try
            {
                return RuntimeHelper.GetRootGameObjects(scene);
            }
            catch (MissingMethodException ex)
            {
                providerRootsBroken = true;
                ExplorerCore.LogWarning($"RuntimeHelper.GetRootGameObjects failed, using Scene.GetRootGameObjects() instead: {ex}");
            }
        }
        return scene.GetRootGameObjects();
    }

    private static void UpdateInternal()
    {
        // Inspected scene will exist if it's DontDestroyOnLoad or HideAndDontSave
        int selectedHandle = SelectedScene.HasValue ? GetHandle(SelectedScene.Value) : int.MinValue;
        bool inspectedExists =
            SelectedScene.HasValue
            && ((DontDestroyExists && selectedHandle == DontDestroyHandle)
                || selectedHandle == AssetSceneHandle);

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
            LoadedScenes.Add(DontDestroyScene);
        }
        LoadedScenes.Add(AssetScene);

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
            CurrentRootObjects = GetRootObjects(SelectedScene.Value);
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