using System;
using System.Collections.Generic;

/// <summary>런타임 전용 - JSON과 무관. 실제 Action 델리게이트 보관.</summary>
public class PopupButtonData
{
    public string label;
    public Action onClick;

    public PopupButtonData(string label, Action onClick)
    {
        this.label = label;
        this.onClick = onClick;
    }
}

// ─── JSON 직렬화 전용 구조 ────────────────────────────────────────────────────

[System.Serializable]
public class ButtonJsonData
{
    public string label;
    public string actionId; // "Close", "QuitGame" 등 액션 ID 문자열
}

[System.Serializable]
public class PopupJsonData
{
    public string popupId;
    public string title;
    public string message;
    public bool showCloseButton;
    public List<ButtonJsonData> buttons; // ButtonJsonData 사용 (Action 직렬화 불가)
}

[System.Serializable]
public class PopupDatabase
{
    public List<PopupJsonData> popups; // public이어야 JsonUtility가 읽음
}
