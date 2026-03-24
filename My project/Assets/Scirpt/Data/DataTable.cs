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

    [Serializable]
    private class FoodDataWrapper
    {
        public List<FoodData> foodDataList;
    }

    private List<ItemData> _itemDataList = new List<ItemData>();
    private List<FoodData> _foodDataList = new List<FoodData>();
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

        //FoodData 로드
        TextAsset foodJson = Resources.Load<TextAsset>("Data/Food");
        if (foodJson == null)
        {
            Debug.LogWarning("[DataTable] Resources/Data/Food.json 을 찾을 수 없습니다.");
            return;
        }

        var foodWrapper = JsonUtility.FromJson<FoodDataWrapper>(foodJson.text);
        _foodDataList = foodWrapper.foodDataList ?? new List<FoodData>();


    }

    /// <summary>itemname으로 아이템 단건 조회 (없으면 null)</summary>
    public ItemData GetItem(string itemname)
        => _dict != null && _dict.TryGetValue(itemname, out var item) ? item : null;

    /// <summary>타입별 아이템 목록 반환</summary>
    public List<ItemData> GetByType(ItemType type)
        => _itemDataList.FindAll(i => i.Type == type);

    /// <summary>전체 아이템 목록 (해금 조건 체크 등에 사용)</summary>

    /// 전체 먹이 목록 조희
    public IReadOnlyList<FoodData> AllFoods => _foodDataList;

    public IReadOnlyList<ItemData> AllItems => _itemDataList;
}
