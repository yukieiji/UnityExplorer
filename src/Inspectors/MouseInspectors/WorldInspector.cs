using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace UnityExplorer.Inspectors.MouseInspectors;

public class WorldInspector : MouseInspectorBase
{
    private static Camera? MainCamera;
    private static GameObject? lastHitObject;

    public override void OnBeginMouseInspect()
    {
        if (!EnsureMainCamera())
        {
            ExplorerCore.LogWarning("No valid cameras found! Cannot inspect world!");
        }
    }


    public override void ClearHitData()
    {
        lastHitObject = null;
    }

    public override void OnSelectMouseInspect()
    {
        InspectorManager.Inspect(lastHitObject);
    }

    public override void UpdateMouseInspect(Vector2 mousePos)
    {
        // Attempt to ensure camera each time UpdateMouseInspect is called
        // in case something changed or wasn't set initially.
        var camera = EnsureMainCamera();
        if (camera == null)
        {
            ExplorerCore.LogWarning("No Main Camera was found, unable to inspect world!");
            MouseInspector.Instance.StopInspect();
            return;
        }

        Ray ray = camera.ScreenPointToRay(mousePos);
        Physics.Raycast(ray, out RaycastHit hit, 1000f);

        if (hit.transform)
        {
            OnHitGameObject(hit.transform.gameObject);
        }
        else if (lastHitObject)
        {
            MouseInspector.Instance.ClearHitData();
        }
    }

    internal void OnHitGameObject(GameObject obj)
    {
        if (obj != lastHitObject)
        {
            lastHitObject = obj;
            MouseInspector.Instance.UpdateObjectNameLabel($"<b>Click to Inspect:</b> <color=cyan>{obj.name}</color>");
            MouseInspector.Instance.UpdateObjectPathLabel($"Path: {obj.transform.GetTransformPath(true)}");
        }
    }

    public override void OnEndInspect()
    {
        // not needed
    }


    private static Camera? EnsureMainCamera()
    {
        if (MainCamera != null)
        {
            return MainCamera;
        }

        if (TryGetValidMainCamera(out var camera))
        {
            MainCamera = camera;
            MouseInspector.Instance.UpdateInspectorTitle(
                $"<b>World Inspector ({camera.name})</b> (press <b>ESC</b> to cancel)"
            );
            ExplorerCore.Log($"Using '{camera.transform.GetTransformPath(true)}'");
            return camera;
        }
        return null;
    }

    private static bool TryGetValidMainCamera([NotNullWhen(true)] out Camera? camera)
    {
        camera = Camera.main;
        if (camera != null)
        {
            return true;
        }

        ExplorerCore.LogWarning("No Camera.main found, trying to find a camera named 'Main Camera' or 'MainCamera'...");
        camera = Camera.allCameras.FirstOrDefault(
            c => c.name == "Main Camera" || c.name == "MainCamera");
        if (camera != null)
        {
            return true;
        }

        ExplorerCore.LogWarning("No camera named 'Main Camera' or 'MainCamera' found, using the first camera created...");
        camera = Camera.allCameras.FirstOrDefault();
        if (camera != null)
        {
            return true;
        }

        // If we reach here, no cameras were found at all.
        ExplorerCore.LogWarning("No valid main cameras found!");
        return false;
    }
}