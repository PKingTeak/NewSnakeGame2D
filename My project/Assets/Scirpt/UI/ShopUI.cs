using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    SlimeHead,
    Map,
}

public class ShopUI : BaseUI
{
    [Header("헤더")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private Button   closeButton;

    [Header("탭")]
    [SerializeField] private Button   slimeTabButton;
    [SerializeField] private Button   mapTabButton;
    [SerializeField] private TMP_Text slimeTabText;
    [SerializeField] private TMP_Text mapTabText;

    [Header("아이템 목록")]
    [SerializeField] private Transform   itemContainer;
    [SerializeField] private ShopItemButtonUI itemButtonPrefab;

    [Header("아이템 상세")]
    [SerializeField] private Image    detailIcon;
    [SerializeField] private TMP_Text detailName;
    [SerializeField] private TMP_Text detailDesc;
    [SerializeField] private TMP_Text detailPrice;
    [SerializeField] private Button   actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    // ─── 내부 상태 ────────────────────────────────────────────────────

    private Action                 _onClose;
    private ItemType               _currentTab = ItemType.SlimeHead;
    private ItemData               _selectedItem;
    private List<ShopItemButtonUI> _spawnedButtons = new();

    // ─── BaseUI 오버라이드 ────────────────────────────────────────────


    protected override void OnInitilze()
    {
        if (titleText != null)
            titleText.text = "상점";

        closeButton?.onClick.AddListener(() => _onClose?.Invoke());
        slimeTabButton?.onClick.AddListener(() => SwitchTab(ItemType.SlimeHead));
        mapTabButton?.onClick.AddListener(() => SwitchTab(ItemType.Map));
        actionButton?.onClick.AddListener(OnClickAction);

        base.OnInitilze();
        Hide();
    }

    protected override void OnShow()
    {
        UpdateCoinDisplay();
        SwitchTab(ItemType.SlimeHead);
    }

    // ─── 외부 인터페이스 ──────────────────────────────────────────────

    public void SetOnClose(Action onClose) => _onClose = onClose;

    // ─── 탭 전환 ─────────────────────────────────────────────────────

    private void SwitchTab(ItemType tab)
    {
        _currentTab = tab;

        bool isSlime = tab == ItemType.SlimeHead;
        if (slimeTabText != null) slimeTabText.fontStyle = isSlime  ? FontStyles.Underline : FontStyles.Normal;
        if (mapTabText   != null) mapTabText.fontStyle   = !isSlime ? FontStyles.Underline : FontStyles.Normal;

        RefreshItemList();
    }
    
    // ─── 아이템 목록 갱신 ─────────────────────────────────────────────

    private void RefreshItemList()
    {
        foreach (var btn in _spawnedButtons)
            if (btn != null) Destroy(btn.gameObject);
        _spawnedButtons.Clear();
        _selectedItem = null;

        var items = DataManager.Instance.DataTable.GetByType(_currentTab);
        if (items == null || items.Count == 0) return;

        foreach (var item in items)
        {
            if (itemButtonPrefab == null || itemContainer == null) break;

            var btn = Instantiate(itemButtonPrefab, itemContainer);
            var captured = item;
            btn.Setup(item, IsOwned(item), IsSelected(item), () => OnSelectItem(captured), () => OnBuyItem(captured));
            _spawnedButtons.Add(btn);
        }

        OnSelectItem(items[0]);
    }

    // ─── 아이템 선택 ──────────────────────────────────────────────────

    private void OnSelectItem(ItemData item)
    {
        Debug.Log($"[ShopUI] 아이템 선택: {item.Itemname}");
        _selectedItem = item;
        UpdateDetail(item);
        RefreshButtonSelectionState();
    }

    private void UpdateDetail(ItemData item)
    {
        if (detailIcon != null) detailIcon.sprite = item.Icon;
        if (detailName != null) detailName.text   = item.Itemname;
        if (detailDesc != null) detailDesc.text   = item.ItemInfo;

        bool owned    = IsOwned(item);
        bool selected = IsSelected(item);

        if (detailPrice != null)
            detailPrice.text = owned ? "보유 중" : $"{item.Cost} 코인";

        if (actionButtonText != null)
        {
            if (selected) actionButtonText.text = "선택됨";
            else if (owned) actionButtonText.text = "선택하기";
            else            actionButtonText.text = $"구매 ({item.Cost} 코인)";
        }

        if (actionButton != null)
            actionButton.interactable = !selected;
    }

    private void RefreshButtonSelectionState()
    {
        var items = DataManager.Instance.DataTable.GetByType(_currentTab);
        for (int i = 0; i < _spawnedButtons.Count && i < items.Count; i++)
        {
            _spawnedButtons[i].SetSelected(items[i] == _selectedItem);
        }
    }

    // ─── 구매 / 선택 버튼 ────────────────────────────────────────────

    private void OnBuyItem(ItemData item)
    {
        if (IsOwned(item)) return;

        UIManager.Instance?.ShowPopupById(
            "purchase_confirm",
            contextActions: new Dictionary<string, Action>
            {
                { "ConfirmPurchase", () => ExecutePurchase(item) }
            },
            messageOverride: $"{item.Itemname}\n{item.Cost} 코인으로 구매하시겠습니까?"
        );
    }

    private void OnClickAction()
    {
        Debug.Log("[ShopUI] OnClickAction 호출됨");

        if (_selectedItem == null) { Debug.LogWarning("[ShopUI] _selectedItem이 null"); return; }

        Debug.Log($"[ShopUI] 선택 아이템: {_selectedItem.Itemname}, 보유여부: {IsOwned(_selectedItem)}");
        Debug.Log($"[ShopUI] UIManager.Instance: {UIManager.Instance != null}");

        if (!IsOwned(_selectedItem))
        {
            var captured = _selectedItem;
            UIManager.Instance?.ShowPopupById(
                "purchase_confirm",
                contextActions: new Dictionary<string, Action>
                {
                    { "ConfirmPurchase", () => ExecutePurchase(captured) }
                },
                messageOverride: $"{captured.Itemname}\n{captured.Cost} 코인으로 구매하시겠습니까?"
            );
            return;
        }

        SelectItem(_selectedItem);
    }

    private void ExecutePurchase(ItemData item)
    {
        if (!DataManager.Instance.TryPurchase(item.Itemname, item.Cost))
        {
            UIManager.Instance?.ShowPopupById("not_enough_gold");
            return;
        }

        UpdateCoinDisplay();
        SelectItem(item);
    }

    private void SelectItem(ItemData item)
    {
        if (item.Type == ItemType.SlimeHead)
            DataManager.Instance.SelectHead(item.Itemname);
        else
            DataManager.Instance.SelectMap(item.Itemname);

        UpdateDetail(item);
        RefreshButtonSelectionState();
    }

    // ─── 유틸 ────────────────────────────────────────────────────────

    /// <summary>cost == 0이면 기본 아이템, 아니면 구매 여부 확인</summary>
    private bool IsOwned(ItemData item)
        => item.Cost == 0 || DataManager.Instance.IsPurchased(item.Itemname);

    private bool IsSelected(ItemData item)
    {
        return item.Type == ItemType.SlimeHead
            ? DataManager.Instance.SelectedHeadId == item.Itemname
            : DataManager.Instance.SelectedMapId  == item.Itemname;
    }

    private void UpdateCoinDisplay()
    {
        if (coinText != null)
            coinText.text = $"코인: {DataManager.Instance.Gold}";
    }



}
