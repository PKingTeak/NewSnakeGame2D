using System;
using System.Collections.Generic;
using System.Text;
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

    [Header("구매 적용 버튼")]
    [SerializeField] private Button BuyButton;
    [SerializeField] private Button ApplyButton;
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
        BuyButton?.onClick.AddListener(() => OnBuyItem(_selectedItem));
        ApplyButton?.onClick.AddListener(() => SelectItem(_selectedItem));
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
            bool isRevealed = !item.IsUnlockType || IsOwned(item) || DataManager.Instance.IsUnlocked(item.Itemname);
            btn.Setup(item, IsOwned(item), IsSelected(item), isRevealed, () => OnSelectItem(captured), () => OnBuyItem(captured));
            _spawnedButtons.Add(btn);
        }

        OnSelectItem(items[0]);
    }

    // ─── 아이템 선택 ──────────────────────────────────────────────────

    private void OnSelectItem(ItemData item)
    {
      
        _selectedItem = item;
        UpdateDetail(item);
        RefreshButtonSelectionState();
        //아이템 선택시 적용할지 물어보는 기능 추가 
        //구매 추가 
    }


    private void UpdateDetail(ItemData item)
    {
        if (detailIcon != null) detailIcon.sprite = item.Icon;
        if (detailName != null) detailName.text   = item.Itemname;

        bool owned    = IsOwned(item);
        bool selected = IsSelected(item);

        if (item.IsUnlockType)
        {
            bool unlocked = DataManager.Instance.IsUnlocked(item.Itemname); // 조건 달성 여부
            if (owned)
            {
                // 구매 완료
                if (detailDesc  != null) detailDesc.text  = item.ItemInfo;
                if (detailPrice != null) detailPrice.text = "보유 중";
                if (BuyButton   != null) BuyButton.gameObject.SetActive(false);
            }
            else if (unlocked)
            {
                // 조건 달성 → 골드로 구매 가능
                if (detailDesc  != null) detailDesc.text  = item.ItemInfo + "\n✔ 해금 완료! 구매 가능합니다.";
                if (detailPrice != null) detailPrice.text = $"{item.Cost} 코인";
                if (BuyButton   != null) { BuyButton.gameObject.SetActive(true); BuyButton.interactable = true; }
            }
            else
            {
                // 아직 조건 미달성 → 진행도 표시
                if (detailDesc  != null) detailDesc.text  = BuildUnlockProgressText(item);
                if (detailPrice != null) detailPrice.text = "조건 미달성";
                if (BuyButton   != null) BuyButton.interactable =false;
            }
        }
        else
        {
            // 골드 구매 아이템
            if (detailDesc  != null) detailDesc.text  = item.ItemInfo;
            if (detailPrice != null) detailPrice.text = owned ? "보유 중" : $"{item.Cost} 코인";
            if (BuyButton   != null)
            {
                BuyButton.gameObject.SetActive(true);
                BuyButton.interactable = !owned;
            }
        }

        if (ApplyButton != null)
            ApplyButton.interactable = owned && !selected;
    }

    private static readonly string[] FoodNames = { "불", "풀", "얼음" };

    private string BuildUnlockProgressText(ItemData item)
    {
        var sb = new StringBuilder();
        sb.AppendLine("???");
        sb.AppendLine("[해금 조건] (해당 라운드 먹이 획득)");
        foreach (var cond in item.UnlockConditions)
        {
            string foodName = cond.foodType < FoodNames.Length ? FoodNames[cond.foodType] : $"먹이{cond.foodType}";
            int session = DataManager.Instance.GetSessionFoodCount((FoodType)cond.foodType);
            int total   = DataManager.Instance.GetFoodEatenCount((FoodType)cond.foodType);
            sb.AppendLine($"  {foodName}: 해당 라운드 {session}/{cond.requiredCount}");
        }
        return sb.ToString().TrimEnd();
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
        // 해금형 아이템은 조건 달성(IsUnlocked) 후에만 구매 가능
        if (item.IsUnlockType && !DataManager.Instance.IsUnlocked(item.Itemname)) return;

        UIManager.Instance?.ShowPopupById(
            "purchase_confirm",
            contextActions: new Dictionary<string, Action>
            {
                { "ConfirmPurchase", () => ExecutePurchase(item) }
            },
            messageOverride: $"{item.Itemname}\n{item.Cost} 코인으로 구매하시겠습니까?"
        );
    }

    private void ExecutePurchase(ItemData item)
    {
        if (!DataManager.Instance.TryPurchase(item.Itemname, item.Cost))
        {
            UIManager.Instance?.ShowPopupById("not_enough_gold");
            return;
        }

        SoundManager.Instance?.PlaySfx(SfxType.Purchase);
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

    private bool IsOwned(ItemData item)
    {
        // 해금형: 조건 달성 후 골드로 구매해야 소유
        if (item.IsUnlockType)
            return DataManager.Instance.IsPurchased(item.Itemname);
        return item.Cost == 0 || DataManager.Instance.IsPurchased(item.Itemname);
    }

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
