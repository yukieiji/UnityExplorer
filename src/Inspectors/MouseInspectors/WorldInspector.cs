namespace UnityExplorer.Inspectors.MouseInspectors
{
    public class WorldInspector : MouseInspectorBase
    {
        private static Camera MainCamera;
        private static GameObject lastHitObject;

        public override void OnBeginMouseInspect()
        {
            if (!EnsureMainCamera())
            {
                ExplorerCore.LogWarning("No valid cameras found! Cannot inspect world!");
            }
        }

        /// <summary>
        /// Checks if the given camera is valid. If it is, assigns it as the MainCamera,
        /// updates the inspector title, logs its usage, and returns true.
        /// Returns false if the camera is invalid.
        /// </summary>
        /// <param name="cam">The camera to validate and possibly assign.</param>
        /// <returns>True if the camera is valid and assigned as MainCamera; false otherwise.</returns>
        private static bool IsValidCam(Camera cam)
        {
            if (!cam) return false;

            MainCamera = cam;
            MouseInspector.Instance.UpdateInspectorTitle(
                $"<b>World Inspector ({MainCamera.name})</b> (press <b>ESC</b> to cancel)"
            );
            ExplorerCore.Log($"Using '{MainCamera.transform.GetTransformPath(true)}'");
            return true;
        }
        
        public override void ClearHitData()
        {
            lastHitObject = null;
        }

        public override void OnSelectMouseInspect()
        {
            InspectorManager.Inspect(lastHitObject);
        }
        
        /// <summary>
        /// Attempts to ensure that MainCamera is assigned. Tries Camera.main, then looks for a camera
        /// named "Main Camera" or "MainCamera", and finally falls back to the first available camera.
        /// If no cameras are available, logs a warning and returns null.
        /// </summary>
        private static Camera EnsureMainCamera()
        {
            if (MainCamera) 
                return MainCamera;

            if (IsValidCam(Camera.main))
                return MainCamera;

            ExplorerCore.LogWarning("No Camera.main found, trying to find a camera named 'Main Camera' or 'MainCamera'...");
            var namedCam = Camera.allCameras.FirstOrDefault(c => c.name is "Main Camera" or "MainCamera");
            if (IsValidCam(namedCam))
                return MainCamera;

            ExplorerCore.LogWarning("No camera named 'Main Camera' or 'MainCamera' found, using the first camera created...");
            var fallbackCam = Camera.allCameras.FirstOrDefault();
            if (IsValidCam(fallbackCam))
                return MainCamera;

            // If we reach here, no cameras were found at all.
            ExplorerCore.LogWarning("No valid cameras found!");
            return null;
        }

        public override void UpdateMouseInspect(Vector2 mousePos)
        {
            // Attempt to ensure camera each time UpdateMouseInspect is called
            // in case something changed or wasn't set initially.
            if (!EnsureMainCamera())
            {
                ExplorerCore.LogWarning("No Main Camera was found, unable to inspect world!");
                MouseInspector.Instance.StopInspect();
                return;
            }

            Ray ray = MainCamera.ScreenPointToRay(mousePos);
            Physics.Raycast(ray, out RaycastHit hit, 1000f);

            if (hit.transform)
                OnHitGameObject(hit.transform.gameObject);
            else if (lastHitObject)
                MouseInspector.Instance.ClearHitData();
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
    }
}