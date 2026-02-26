using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : BaseUI
{
    [Header("Shop Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    private Action _onClose;

    protected override void OnInitilze()
    {
        if (titleText != null && string.IsNullOrWhiteSpace(titleText.text))
        {
            titleText.text = "상점";
        }

        if (descText != null && string.IsNullOrWhiteSpace(descText.text))
        {
            descText.text = "아이템을 구매할 수 있는 상점입니다.";
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        base.OnInitilze();
        Hide();
    }

    public void SetOnClose(Action onClose)
    {
        _onClose = onClose;
    }

    public void Close()
    {
        _onClose?.Invoke();
    }
}
