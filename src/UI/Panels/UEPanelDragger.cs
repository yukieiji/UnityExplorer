using UniverseLib.UI.Panels;

namespace UnityExplorer.UI.Panels
{
    public class UEPanelDragger : PanelDragger
    {
        public UEPanelDragger(PanelBase uiPanel) : base(uiPanel) { }

        protected override void Update(MouseState state, Vector3 rawMousePos)
        {
            if (TouchPanelDrag.IsDragging)
            {
                if (WasDragging)
                    WasDragging = false;

                return;
            }

            base.Update(state, rawMousePos);
        }

        protected override bool MouseInResizeArea(Vector2 mousePos)
        {
            return !UIManager.NavBarRect.rect.Contains(UIManager.NavBarRect.InverseTransformPoint(mousePos))
                && base.MouseInResizeArea(mousePos);
        }
    }
}
