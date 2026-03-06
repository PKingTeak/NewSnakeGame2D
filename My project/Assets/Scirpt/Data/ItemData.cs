using UnityEngine;

[System.Serializable]
public class ItemData
{
    [SerializeField] private string   itemname;
    [SerializeField] private string   itemInfo;
    [SerializeField] private int      cost;
    [SerializeField] private string   iconPath;
    [SerializeField] private ItemType type;
    [SerializeField] private int slimeType;

    public string   Itemname => itemname;
    public string   ItemInfo => itemInfo;
    public int      Cost     => cost;
    public Sprite   Icon     => Resources.Load<Sprite>(iconPath);
    public ItemType Type     => type;
    public int SlimeType => slimeType;
}
