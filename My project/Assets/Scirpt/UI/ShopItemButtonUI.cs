using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 아이템 하나를 표시하는 버튼 UI 컴포넌트.
/// ShopUI에서 Instantiate 후 Setup()으로 초기화.
/// </summary>
public class ShopItemButtonUI : MonoBehaviour
{
    [SerializeField] private Image       iconImage;
    [SerializeField] private TMP_Text    nameText;
    [SerializeField] private GameObject  lockedOverlay;   // 미구매 시 어둡게
    [SerializeField] private GameObject  selectedBorder;  // 선택된 아이템 테두리
    [SerializeField] private Button      button;

    private Action _onClick; //구매버튼 이벤트 연동

    public void Setup(ItemData data, bool isPurchased, bool isSelected, Action onClick)
    {
        _onClick = onClick;

        if (iconImage != null && data.Icon != null)
            iconImage.sprite = data.Icon;

        if (nameText != null)
            nameText.text = data.Itemname;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isPurchased);

        if (selectedBorder != null)
            selectedBorder.SetActive(isSelected);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _onClick?.Invoke());
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(isSelected);
    }


    
}
