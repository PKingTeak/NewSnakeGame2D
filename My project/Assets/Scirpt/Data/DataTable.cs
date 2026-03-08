using System;
using System.Collections.Generic;
using UnityEngine;

public class DataTable
{
    [Serializable]
    private class DataWrapper
    {
        public List<ItemData> itemDataList;
    }

    private List<ItemData>               _itemDataList = new List<ItemData>();
    private Dictionary<string, ItemData> _dict;

    public void Init()
    {
        TextAsset json = Resources.Load<TextAsset>("Data/itemdata");
        if (json == null)
        {
            Debug.LogWarning("[DataTable] Resources/itemdata.json 을 찾을 수 없습니다.");
            return;
        }

        var wrapper = JsonUtility.FromJson<DataWrapper>(json.text);
        _itemDataList = wrapper.itemDataList ?? new List<ItemData>();

        _dict = new Dictionary<string, ItemData>();
        foreach (var item in _itemDataList)
        {
            if (!string.IsNullOrEmpty(item.Itemname))
                _dict[item.Itemname] = item;
        }

        Debug.Log($"[DataTable] 아이템 {_itemDataList.Count}개 로드 완료.");
    }

    /// <summary>itemname으로 아이템 단건 조회 (없으면 null)</summary>
    public ItemData GetItem(string itemname)
        => _dict != null && _dict.TryGetValue(itemname, out var item) ? item : null;

    /// <summary>타입별 아이템 목록 반환</summary>
    public List<ItemData> GetByType(ItemType type)
        => _itemDataList.FindAll(i => i.Type == type);
}
