using UniverseLib.UI;

namespace UnityExplorer.UI.Panels
{
    public class ClipboardPanel : UEPanel
    {
        public static List<object> ClipboardItems = new List<object>();
        public static int selectedItem = 0;
        public static ClipboardPanel Instance;
        public override string Name => "Clipboard";
        public override UIManager.Panels PanelType => UIManager.Panels.Clipboard;

        public override int MinWidth => 500;
        public override int MinHeight => 95;
        public override Vector2 DefaultAnchorMin => new(0.1f, 0.05f);
        public override Vector2 DefaultAnchorMax => new(0.4f, 0.15f);

        public override bool CanDragAndResize => true;
        public override bool NavButtonWanted => true;
        public override bool ShouldSaveActiveState => true;
        public override bool ShowByDefault => true;

        private static List<GameObject> clipboardRows = new List<GameObject>();

        public ClipboardPanel(UIBase owner) : base(owner)
        {
        }

        public static void SelectClipboardItem(int index)
        {
            selectedItem = index;
        }

        /// <summary>
        /// Adds a new entry and selects it
        /// </summary>
        public static void AddNewEntry()
        {
            ClipboardItems.Add(null);
            SelectClipboardItem(ClipboardItems.Count - 1);
            UpdateCurrentPasteInfo();
        }
        public static void Copy(object obj)
        {
            if (obj == null || ClipboardItems.Count <= selectedItem)
            {
                ExplorerCore.Log($"The selected item index is outside the range! {ClipboardItems.Count} is the count.");
                return;
            }
            Copy(obj,selectedItem);
        }

        public static void Copy(object obj, int index)
        {
            if (obj == null)
            {
                return;
            }
            if (ClipboardItems.Count() <= selectedItem)
            {
                ClipboardItems.Insert(index,obj);
            }
            ClipboardItems[index] = obj;
            Notification.ShowMessage("Copied to clipboard at index " + selectedItem + "!");
            UpdateCurrentPasteInfo();
        }

        public static void CopyToEnd(object obj)
        {
            ClipboardItems.Add(obj);
            UpdateCurrentPasteInfo();
        }

        public static void RemoveIndex(int i)
        {
            ClipboardItems.RemoveAt(i);
            UpdateCurrentPasteInfo();
        }

        public static bool TryPaste(Type targetType, out object paste)
        {
            paste = ClipboardItems[selectedItem];
            Type pasteType = ClipboardItems?.GetActualType();

            if (ClipboardItems != null && !targetType.IsAssignableFrom(pasteType))
            {
                Notification.ShowMessage($"Cannot assign '{pasteType.Name}' to '{targetType.Name}'!");
                return false;
            }

            Notification.ShowMessage("Pasted!");
            return true;
        }

        public static void ClearClipboard()
        {
            ClipboardItems.Clear();
            UpdateCurrentPasteInfo();
        }

        private static void UpdateCurrentPasteInfo()
        {
            Instance.RefreshList();
        }

        private static void InspectClipboard()
        {
            if (ClipboardItems.Count <= selectedItem)
            {
                //Something has gone wrong, this shouldn't happen!
                return;
            }
            
            if (ClipboardItems[selectedItem].IsNullOrDestroyed())
            {
                Notification.ShowMessage("Cannot inspect a null or destroyed object!");
                return;
            }

            InspectorManager.Inspect(ClipboardItems[selectedItem]);
        }

        public override void SetDefaultSizeAndPosition()
        {
            base.SetDefaultSizeAndPosition();

            this.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MinWidth);
            this.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, MinHeight);
        }
        protected override void ConstructPanelContent()
        {
            this.UIRoot.GetComponent<Image>().color = new(0.1f, 0.1f, 0.1f);
            ClipboardPanel.Instance = this;
            
            // Actual panel content

            GameObject firstRow = UIFactory.CreateHorizontalGroup(ContentRoot, "FirstRow", false, false, true, true, 5, new(2, 2, 2, 2), new(1, 1, 1, 0));
            UIFactory.SetLayoutElement(firstRow, minHeight: 25, flexibleWidth: 999);

            // Title for "Current Paste:"
            Text currentPasteTitle = UIFactory.CreateLabel(firstRow, "CurrentPasteTitle", "Current paste:", TextAnchor.MiddleLeft, color: Color.grey);
            UIFactory.SetLayoutElement(currentPasteTitle.gameObject, minHeight: 25, minWidth: 100, flexibleWidth: 999);

            // Clear clipboard button
            UniverseLib.UI.Models.ButtonRef clearButton = UIFactory.CreateButton(firstRow, "ClearPasteButton", "Clear Clipboard");
            UIFactory.SetLayoutElement(clearButton.Component.gameObject, minWidth: 120, minHeight: 25, flexibleWidth: 0);
            clearButton.OnClick += ClearClipboard;
            
            // Inspect Selected button
            UniverseLib.UI.Models.ButtonRef inspectButton = UIFactory.CreateButton(firstRow, "InspectButton", "Inspect");
            UIFactory.SetLayoutElement(inspectButton.Component.gameObject, minHeight: 25, flexibleHeight: 0, minWidth: 80, flexibleWidth: 0);
            inspectButton.OnClick += InspectClipboard;
            
            // Add New button
            UniverseLib.UI.Models.ButtonRef addNewButton = UIFactory.CreateButton(firstRow, "AddNewButton", "Add");
            UIFactory.SetLayoutElement(addNewButton.Component.gameObject, minHeight: 25, flexibleHeight: 0, minWidth: 80, flexibleWidth: 0);
            addNewButton.OnClick += AddNewEntry;

            RefreshList();
        }

        public void RefreshList()
        {
            foreach (GameObject go in clipboardRows)
            {
                //Remove the row
                GameObject.Destroy(go);
            }
            clipboardRows.Clear();
            
            for (int i = 0; i < ClipboardItems.Count; i++)
            {
                // Current Paste info row
                GameObject thisClipboardRow = UIFactory.CreateHorizontalGroup(ContentRoot, "SecondRow", false, false, true, true, 0,
                    new(2, 2, 2, 2), childAlignment: TextAnchor.UpperCenter);

                // Actual current paste info label
                Text label = UIFactory.CreateLabel(thisClipboardRow, "CurrentPasteInfo", "not set", TextAnchor.UpperLeft);
                UIFactory.SetLayoutElement(label.gameObject, minHeight: 25, minWidth: 100, flexibleWidth: 999, flexibleHeight: 999);
                if (ClipboardItems[i] != null)
                {
                    label.text = ToStringUtility.ToStringWithType(ClipboardItems[i], typeof(object), false);
                }
                
                // Remove button
                UniverseLib.UI.Models.ButtonRef removeThis = UIFactory.CreateButton(thisClipboardRow, "RemoveButton", "Remove");
                UIFactory.SetLayoutElement(removeThis.Component.gameObject, minHeight: 25, flexibleHeight: 0, minWidth: 80, flexibleWidth: 0);
                
                
                // Select button
                UniverseLib.UI.Models.ButtonRef selectThis = UIFactory.CreateButton(thisClipboardRow, "SelectButton", "Select");
                UIFactory.SetLayoutElement(selectThis.Component.gameObject, minHeight: 25, flexibleHeight: 0, minWidth: 80, flexibleWidth: 0);
                
                //This should select a clipboard item...
                int thisItemIndexInList = i;
                selectThis.OnClick += () => SelectClipboardItem(thisItemIndexInList);
                removeThis.OnClick += () => RemoveIndex(thisItemIndexInList);
                
                clipboardRows.Add(thisClipboardRow);
            }
        }
    }
}
