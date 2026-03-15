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

    [SerializeField] private Button buyButton;

    private Action _onClick;
    private Action _onBuy;

    public void OnPointerClick(PointerEventData eventData)
    {
        _onClick?.Invoke();
    }

    public void Setup(ItemData data, bool isPurchased, bool isSelected, Action onClick, Action onBuy = null)
    {
        _onClick = onClick;
        _onBuy   = onBuy;

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

        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(!isPurchased);
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => _onBuy?.Invoke());
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(isSelected);
    }

   
    
}
