using System.Collections.Generic;
using UnityEngine;

/// <summary>해금 조건: 특정 먹이 타입을 n개 먹어야 함.</summary>
[System.Serializable]
public class UnlockCondition
{
    public int foodType;      // FoodType enum 값 (0=Red, 1=Green, 2=Blue)
    public int requiredCount; // 먹어야 하는 수
}
[System.Serializable]
public class ComboBonusEntry
{
    public int foodType;
    public int BonusPoints;

}


[System.Serializable]
public class ItemData
{
    [SerializeField] private string   itemname;
    [SerializeField] private string   itemInfo;
    [SerializeField] private int      cost;
    [SerializeField] private string   iconPath;
    [SerializeField] private ItemType type;
    [SerializeField] private int  slimeType;

    [SerializeField] private List<UnlockCondition> unlockConditions;

    [SerializeField] private List<ComboBonusEntry> comboBonuses;

    public string   Itemname  => itemname;
    public string   ItemInfo  => itemInfo;
    public int      Cost      => cost;
    public Sprite   Icon      => Resources.Load<Sprite>(iconPath);
    public ItemType Type      => type;

    public int      SlimeType => slimeType;


    /// <summary>해금 조건 목록. 비어있으면 골드 구매 아이템.</summary>
    public List<UnlockCondition> UnlockConditions => unlockConditions;

    /// <summary>조건 달성으로 해금하는 아이템이면 true.</summary>
    public bool IsUnlockType => unlockConditions != null && unlockConditions.Count > 0;

    ///<summary>콤보 보너스 목록. 비어있으면 콤보 보너스 없음.</summary>
    public List<ComboBonusEntry> ComboBonuses => comboBonuses;
}
