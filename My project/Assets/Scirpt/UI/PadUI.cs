using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PadUI : MonoBehaviour
{
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    private void Awake()
    {
#if !UNITY_EDITOR && !UNITY_IOS && !UNITY_ANDROID
        gameObject.SetActive(false);
#endif
    }

    private void Start()
    {
        AddPointerDown(upButton,    Vector2.up);
        AddPointerDown(downButton,  Vector2.down);
        AddPointerDown(leftButton,  Vector2.left);
        AddPointerDown(rightButton, Vector2.right);
    }

    private void AddPointerDown(Button btn, Vector2 dir)
    {
        if (btn == null) return;
        var trigger = btn.gameObject.GetComponent<EventTrigger>()
                   ?? btn.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entry.callback.AddListener(_ => OnClickButton(dir));
        trigger.triggers.Add(entry);
    }

    public void OnClickButton(Vector2 dir)
    {
        GameManager.Instance.SlimeController?.SetInputDirection(dir);
    }
}
