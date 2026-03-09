using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 상점 아이템 하나를 표시하는 버튼 UI 컴포넌트.
/// ShopUI에서 Instantiate 후 Setup()으로 초기화.
/// </summary>
public class ShopItemButtonUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image       iconImage;
    [SerializeField] private TMP_Text    nameText;
    [SerializeField] private TMP_Text    priceText;
    [SerializeField] private GameObject  lockedOverlay;
    [SerializeField] private GameObject  selectedBorder;

    private Action _onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        _onClick?.Invoke();
    }

    public void Setup(ItemData data, bool isPurchased, bool isSelected, Action onClick)
    {
        _onClick = onClick;

        if (iconImage != null && data.Icon != null)
            iconImage.sprite = data.Icon;

        if (nameText != null)
            nameText.text = data.Itemname;

        if (priceText != null)
            priceText.text = data.Cost.ToString();

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isPurchased);

        if (selectedBorder != null)
            selectedBorder.SetActive(isSelected);
    }

    public void SetSelected(bool isSelected)
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(isSelected);
    }


    
}
