using UnityEngine;

/// <summary>
/// 상점 데이터를 PlayerPrefs에 저장/로드하는 정적 유틸리티 클래스.
/// MonoBehaviour 불필요 - 어디서든 직접 호출 가능.
/// </summary>
public static class ShopDataManager
{
    private const string CoinKey          = "Shop_Coins";
    private const string PurchasedKey     = "Shop_Purchased";
    private const string SelectedHeadKey  = "Shop_SelectedHead";
    private const string SelectedMapKey   = "Shop_SelectedMap";

    public const string DefaultHeadId = "slime_default";
    public const string DefaultMapId  = "map_default";

    // ─── 코인 ───────────────────────────────────────────────────────

    public static int Coins => PlayerPrefs.GetInt(CoinKey, 0);

    public static void AddCoins(int amount)
    {
        if (amount <= 0) return;
        PlayerPrefs.SetInt(CoinKey, Coins + amount);
        PlayerPrefs.Save();
    }

    // ─── 구매 여부 ───────────────────────────────────────────────────

    /// <summary>itemId가 구매됐는지 확인. (기본 아이템은 ShopItemData.isDefault 로 별도 처리)</summary>
    public static bool IsPurchased(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        string saved = PlayerPrefs.GetString(PurchasedKey, "");
        if (string.IsNullOrEmpty(saved)) return false;

        foreach (var id in saved.Split(','))
        {
            if (id == itemId) return true;
        }
        return false;
    }

    /// <summary>코인이 충분하면 구매 처리 후 true 반환.</summary>
    public static bool TryPurchase(string itemId, int price)
    {
        if (Coins < price) return false;
        if (IsPurchased(itemId)) return false;

        PlayerPrefs.SetInt(CoinKey, Coins - price);

        string saved = PlayerPrefs.GetString(PurchasedKey, "");
        string updated = string.IsNullOrEmpty(saved) ? itemId : saved + "," + itemId;
        PlayerPrefs.SetString(PurchasedKey, updated);
        PlayerPrefs.Save();
        return true;
    }

    // ─── 선택 ────────────────────────────────────────────────────────

    public static string SelectedHeadId => PlayerPrefs.GetString(SelectedHeadKey, DefaultHeadId);
    public static string SelectedMapId  => PlayerPrefs.GetString(SelectedMapKey,  DefaultMapId);

    public static void SelectHead(string itemId)
    {
        PlayerPrefs.SetString(SelectedHeadKey, itemId);
        PlayerPrefs.Save();
    }

    public static void SelectMap(string itemId)
    {
        PlayerPrefs.SetString(SelectedMapKey, itemId);
        PlayerPrefs.Save();
    }
}
