using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityExplorer.UI.Panels;
using UniverseLib.UI;
using UniverseLib.UI.Panels;

namespace UnityExplorer.UI
{
    internal static class TouchPanelDrag
    {
        internal static bool IsDragging => draggingPanel != null;

        private const string CLOSE_BUTTON_HOLDER = "CloseHolder";
        
        private const int PHASE_BEGAN = 0;
        private const int PHASE_ENDED = 3;
        private const int PHASE_CANCELED = 4;
        
        private static bool setupDone;
        private static bool setupFailed;
        private static PropertyInfo pTouchCount;
        private static MethodInfo mGetTouch;
        private static Type touchType;
        private static PropertyInfo pPosition;
        private static PropertyInfo pPhase;
        private static PropertyInfo pFingerId;
        private static FieldInfo fPosition;
        private static FieldInfo fPhase;
        private static FieldInfo fFingerId;

        private static UEPanel draggingPanel;
        private static int draggingFinger = -1;
        private static Vector2 lastScreenPos;

        internal static void Update()
        {
            if (!Setup())
                return;

            if (draggingPanel != null && (!draggingPanel.Enabled || draggingPanel.UIRoot == null))
            {
                EndDrag(false);
                return;
            }

            if (!UIManager.ShowMenu || UIManager.UICanvas == null)
            {
                if (draggingPanel != null)
                    EndDrag(false);
                return;
            }

            int touchCount;
            try
            {
                touchCount = (int)pTouchCount.GetValue(null, null);
            }
            catch (Exception ex)
            {
                // Unity throws here when the project uses the new Input System only.
                setupFailed = true;
                ExplorerCore.LogWarning($"Touch dragging disabled, could not read touch count: {ex.Message}");
                return;
            }

            if (touchCount <= 0)
            {
                if (draggingPanel != null)
                    EndDrag(true);
                return;
            }

            for (int i = 0; i < touchCount; i++)
            {
                object touch;
                try
                {
                    touch = mGetTouch.Invoke(null, new object[] { i });
                }
                catch
                {
                    continue;
                }

                if (touch == null)
                    continue;

                int fingerId = GetFingerId(touch);
                int phase = GetPhase(touch);
                Vector2 pos = GetPosition(touch);

                if (draggingPanel == null)
                {
                    if (phase != PHASE_BEGAN)
                        continue;

                    UEPanel panel = FindPanelAt(pos);
                    if (panel == null)
                        continue;

                    BeginDrag(panel, fingerId, pos);
                }
                else if (fingerId == draggingFinger)
                {
                    if (phase == PHASE_ENDED || phase == PHASE_CANCELED)
                    {
                        EndDrag(true);
                        return;
                    }

                    MoveDrag(pos);
                }
            }
        }

        // Drag lifecycle

        private static void BeginDrag(UEPanel panel, int fingerId, Vector2 screenPos)
        {
            draggingPanel = panel;
            draggingFinger = fingerId;
            lastScreenPos = screenPos;

            try
            {
                // Bring the panel to the front, same as the mouse path does on mouse-down.
                panel.SetActive(true);
            }
            catch (Exception ex)
            {
                ExplorerCore.LogWarning($"Exception focusing panel '{panel.Name}': {ex.Message}");
            }

            ClearSimulatedMouseDrag();
        }

        private static void MoveDrag(Vector2 screenPos)
        {
            if (draggingPanel == null)
                return;

            Vector2 delta = screenPos - lastScreenPos;
            lastScreenPos = screenPos;

            if (delta.sqrMagnitude <= 0f)
                return;

            float scale = UIManager.UICanvas ? UIManager.UICanvas.scaleFactor : 1f;
            if (scale <= 0f)
                scale = 1f;

            try
            {
                RectTransform rect = draggingPanel.Rect;
                rect.localPosition += (Vector3)(delta / scale);
                ClampToCanvas(rect);
            }
            catch (Exception ex)
            {
                ExplorerCore.LogWarning($"Exception dragging panel '{draggingPanel?.Name}': {ex.Message}");
                EndDrag(false);
                return;
            }

            // Keep the legacy mouse dragger out of the way while the finger owns this panel.
            ClearSimulatedMouseDrag();
        }

        private static void EndDrag(bool save)
        {
            UEPanel panel = draggingPanel;
            draggingPanel = null;
            draggingFinger = -1;

            if (panel == null || !save)
                return;

            try
            {
                panel.OnFinishDrag();
            }
            catch (Exception ex)
            {
                ExplorerCore.LogWarning($"Exception saving panel '{panel.Name}' position: {ex.Message}");
            }
        }

        private static void ClampToCanvas(RectTransform rect)
        {
            RectTransform holder = rect.parent as RectTransform;
            if (holder == null)
                return;

            Vector2 holderSize = holder.rect.size;
            float halfW = holderSize.x * 0.5f;
            float halfH = holderSize.y * 0.5f;

            const float KEEP_VISIBLE = 50f;

            Vector3 pos = rect.localPosition;
            pos.x = Mathf.Clamp(pos.x, -halfW - rect.rect.width + KEEP_VISIBLE, halfW - KEEP_VISIBLE);
            pos.y = Mathf.Clamp(pos.y, -halfH + KEEP_VISIBLE, halfH);
            rect.localPosition = pos;
        }
        
        private static void ClearSimulatedMouseDrag()
        {
            foreach (UEPanel panel in UIManager.UIPanels.Values)
            {
                if (panel?.Dragger != null && panel.Dragger.WasDragging)
                    panel.Dragger.WasDragging = false;
            }
        }

        // Hit testing

        private static UEPanel FindPanelAt(Vector2 screenPos)
        {
            UEPanel best = null;
            int bestIndex = int.MinValue;

            foreach (UEPanel panel in UIManager.UIPanels.Values)
            {
                if (panel?.UIRoot == null || !panel.Enabled || !panel.CanDragAndResize)
                    continue;

                GameObject titleBar = panel.TitleBar;
                if (titleBar == null || !titleBar.activeInHierarchy)
                    continue;

                RectTransform titleRect = titleBar.GetComponent<RectTransform>();
                if (!ContainsScreenPoint(titleRect, screenPos))
                    continue;

                // Don't start a drag from the close button, that would also close the panel.
                Transform closeHolder = titleBar.transform.Find(CLOSE_BUTTON_HOLDER);
                if (closeHolder is RectTransform closeRect && ContainsScreenPoint(closeRect, screenPos))
                    continue;

                int index = panel.UIRoot.transform.GetSiblingIndex();
                if (index > bestIndex)
                {
                    bestIndex = index;
                    best = panel;
                }
            }

            return best;
        }

        private static bool ContainsScreenPoint(RectTransform rect, Vector2 screenPos)
        {
            if (rect == null)
                return false;

            try
            {
                return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, UICamera);
            }
            catch
            {
                return false;
            }
        }

        private static Camera UICamera
        {
            get
            {
                Canvas canvas = UIManager.UICanvas;
                if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    return null;

                return canvas.worldCamera;
            }
        }

        // Reflection setup / helpers

        private static bool Setup()
        {
            if (setupDone)
                return !setupFailed;

            setupDone = true;

            try
            {
                Type inputType = UniverseLib.Input.LegacyInput.TInput;
                if (inputType == null)
                {
                    setupFailed = true;
                    ExplorerCore.LogWarning("Touch dragging unavailable: UnityEngine.Input was not found.");
                    return false;
                }

                pTouchCount = inputType.GetProperty("touchCount", BindingFlags.Public | BindingFlags.Static);
                mGetTouch = inputType.GetMethod("GetTouch", BindingFlags.Public | BindingFlags.Static,
                    null, new[] { typeof(int) }, null);

                if (pTouchCount == null || mGetTouch == null)
                {
                    setupFailed = true;
                    ExplorerCore.LogWarning("Touch dragging unavailable: Input.touchCount / GetTouch missing.");
                    return false;
                }

                touchType = mGetTouch.ReturnType;

                pPosition = touchType.GetProperty("position");
                pPhase = touchType.GetProperty("phase");
                pFingerId = touchType.GetProperty("fingerId");

                // Interop structs sometimes expose the members as fields instead of properties.
                if (pPosition == null)
                    fPosition = touchType.GetField("position");
                if (pPhase == null)
                    fPhase = touchType.GetField("phase");
                if (pFingerId == null)
                    fFingerId = touchType.GetField("fingerId");

                if ((pPosition == null && fPosition == null) ||
                    (pPhase == null && fPhase == null) ||
                    (pFingerId == null && fFingerId == null))
                {
                    setupFailed = true;
                    ExplorerCore.LogWarning($"Touch dragging unavailable: unexpected layout of {touchType.FullName}.");
                }

                return !setupFailed;
            }
            catch (Exception ex)
            {
                setupFailed = true;
                ExplorerCore.LogWarning($"Touch dragging unavailable: {ex.Message}");
                return false;
            }
        }

        private static object GetMember(object touch, PropertyInfo prop, FieldInfo field)
        {
            if (prop != null)
                return prop.GetValue(touch, null);
            if (field != null)
                return field.GetValue(touch);

            return null;
        }

        private static Vector2 GetPosition(object touch)
        {
            try
            {
                object value = GetMember(touch, pPosition, fPosition);
                return value is Vector2 vector ? vector : Vector2.zero;
            }
            catch
            {
                return Vector2.zero;
            }
        }

        private static int GetPhase(object touch)
        {
            try
            {
                object value = GetMember(touch, pPhase, fPhase);
                return value == null ? PHASE_CANCELED : Convert.ToInt32(value);
            }
            catch
            {
                return PHASE_CANCELED;
            }
        }

        private static int GetFingerId(object touch)
        {
            try
            {
                object value = GetMember(touch, pFingerId, fFingerId);
                return value == null ? -1 : Convert.ToInt32(value);
            }
            catch
            {
                return -1;
            }
        }
    }
}
