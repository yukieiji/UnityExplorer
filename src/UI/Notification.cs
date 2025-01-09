using JetBrains.Annotations;
using UniverseLib.UI;

#nullable enable

namespace UnityExplorer.UI;

public sealed class Notification
{
    private readonly Text popupLabel;
    private float _timeOfLastNotification;

    private static Notification? _instance;

    public static void Init(GameObject parent)
    {
        if (_instance == null)
        {
            _instance = new Notification(parent);
        }
    }

    private Notification(GameObject parent)
    {
        popupLabel = UIFactory.CreateLabel(parent, "ClipboardNotification", "", TextAnchor.MiddleCenter);
        popupLabel.rectTransform.sizeDelta = new(500, 100);
        popupLabel.gameObject.AddComponent<Outline>();
        CanvasGroup popupGroup = popupLabel.gameObject.AddComponent<CanvasGroup>();
        popupGroup.blocksRaycasts = false;
    }

    private void ShowMessageImp(string message)
    {
        popupLabel.text = message;
        _timeOfLastNotification = Time.realtimeSinceStartup;

        popupLabel.transform.localPosition = UIManager.UIRootRect.InverseTransformPoint(DisplayManager.MousePosition) + (Vector3.up * 25);
    }

    private void UpdateImp()
    {
        if (popupLabel.text != string.Empty &&
            Time.realtimeSinceStartup - _timeOfLastNotification > 2f)
        {
            popupLabel.text = string.Empty;
        }
    }

    public static void ShowMessage(string message)
        => _instance?.ShowMessageImp(message);

    public static void Update()
        => _instance?.UpdateImp();
}
