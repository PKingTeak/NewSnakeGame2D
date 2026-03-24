using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoSingleton<DataManager>
{
    public readonly DataTable DataTable = new DataTable();  //모든 데이터를 담든 창고 느낌 

    [Serializable]
    public class PlayerData
    {
        public string playerName     = "Player";
        public int    gold           = 0;
        public int    highScore      = 0;
        public List<string> purchasedItems = new List<string>();
        public string selectedHeadId = "slime_base";
        public string selectedMapId  = "map_default";

        // 먹이 타입별 누적 섭취 횟수 (FoodType: Red=0, Green=1, Blue=2)
        public int[] foodEatenCounts = new int[3];
        // 조건 달성으로 해금된 아이템 ID 목록
        public List<string> unlockedItems = new List<string>();


        // 사운드 볼륨 (0~1)
        public float bgmVolume = 1f;
        public float sfxVolume = 1f;
    }

    private PlayerData _data;
    // 세션(한 게임) 동안만 유지되는 먹이 카운트. 저장되지 않음.
    private int[] _sessionFoodCounts = new int[3];

    /// <summary>현재 로드된 플레이어 데이터 (읽기 전용).</summary>
    public PlayerData Data => _data;

    private string SavePath => Path.Combine(Application.persistentDataPath, "playerdata.json"); //책임 분리해야할듯
    //Json전용 저장소를 만들어야할거 같음. 
    //전체 데이터가 로드된 데이터 테이블 같은것을 만들면 좋을거 같음.

    // ─────────────────────────────────────────────────────────────────────────
    // 초기화 / 저장·로드
    //     // ─────────────────────────────────────────────────────────────────────────
    

   
    protected override void Awake()
    {
        base.Awake();
        DataTable.Init();
        EventHub.Subscribe(AddGold);
        Load();
    }

   
    void OnDestroy()
    {
        EventHub.Unsubscribe(AddGold);
    }

    public void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            _data = JsonUtility.FromJson<PlayerData>(json);
            // 구버전 세이브 파일 호환: 필드 누락 대비
            if (_data.foodEatenCounts == null || _data.foodEatenCounts.Length < 3)
                _data.foodEatenCounts = new int[3];
            if (_data.unlockedItems == null)
                _data.unlockedItems = new List<string>();
            Debug.Log($"[DataManager] 데이터 로드 완료: {SavePath}");
        }
        else
        {
            _data = new PlayerData();
            Save();
            Debug.Log("[DataManager] 새 데이터 파일 생성.");
        }
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(_data, prettyPrint: true);
        File.WriteAllText(SavePath, json);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 플레이어 이름
    // ─────────────────────────────────────────────────────────────────────────

    public string PlayerName => _data.playerName;

    public void SetPlayerName(string name)
    {
        _data.playerName = name;
        Save();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 골드
    // ─────────────────────────────────────────────────────────────────────────

    public int Gold => _data.gold;

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        _data.gold += amount;
        Save();
    }

    /// <summary>골드가 충분하면 차감 후 true 반환. 부족하면 false.</summary>
    public bool SpendGold(int amount)
    {
        if (_data.gold < amount) return false;
        _data.gold -= amount;
        Save();
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 최고 점수
    // ─────────────────────────────────────────────────────────────────────────

    public int HighScore => _data.highScore;

    /// <summary>현재 점수가 최고 기록을 넘으면 갱신 후 true 반환.</summary>
    public bool TryUpdateHighScore(int score)
    {
        if (score <= _data.highScore) return false;
        _data.highScore = score;
        Save();
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 구매 아이템
    // ─────────────────────────────────────────────────────────────────────────

    public bool IsPurchased(string itemId) => _data.purchasedItems.Contains(itemId);

    /// <summary>골드가 충분하고 미구매 아이템이면 구매 처리 후 true 반환.</summary>
    public bool TryPurchase(string itemId, int price)
    {
        if (IsPurchased(itemId)) return false;
        if (!SpendGold(price)) return false;     // SpendGold 내부에서 Save() 호출
        _data.purchasedItems.Add(itemId);
        Save();
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 선택 스킨
    // ─────────────────────────────────────────────────────────────────────────

    public string SelectedHeadId => _data.selectedHeadId;
    public string SelectedMapId  => _data.selectedMapId;

    public void SelectHead(string itemId) { _data.selectedHeadId = itemId; Save(); }
    public void SelectMap(string itemId)  { _data.selectedMapId  = itemId; Save(); }

    // ─────────────────────────────────────────────────────────────────────────
    // 먹이 섭취 추적 & 해금 시스템
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>누적 먹이 횟수 조회 (전체 게임 합산).</summary>
    public int GetFoodEatenCount(FoodType foodType)
    {
        int idx = (int)foodType;
        if (idx < 0 || idx >= _data.foodEatenCounts.Length) return 0;
        return _data.foodEatenCounts[idx];
    }

    ///<summary>해금한 먹이들 조회</summary>
    public FoodType[] GetAvailableFoodType()
    {
        List<FoodType> foods = new List<FoodType>
        {
            FoodType.fire,
            FoodType.grass,
            FoodType.ice
        };

        foreach(FoodData food in DataTable.AllFoods)
        {
            if(IsPurchased(food.UnlockedBySlime))
            {
                foods.Add((FoodType)food.EnumIndex);
            }
        }

        return foods.ToArray();
    }
     
    /// <summary>현재 세션(이번 한 게임)의 먹이 횟수 조회.</summary>
    public int GetSessionFoodCount(FoodType foodType)
    {
        int idx = (int)foodType;
        if (idx < 0 || idx >= _sessionFoodCounts.Length) return 0;
        return _sessionFoodCounts[idx];
    }

    /// <summary>해금 조건이 달성돼 상점 구매가 가능한 상태인지 확인.</summary>
    public bool IsUnlocked(string itemId) => _data.unlockedItems.Contains(itemId);

    /// <summary>새 게임 시작 시 호출. 세션 카운트를 초기화한다.</summary>
    public void ResetSessionCounts() => _sessionFoodCounts = new int[3];

    /// <summary>먹이 섭취 시 호출. 누적/세션 카운트를 모두 올리고 해금 조건을 체크한다.</summary>
    public void AddFoodEaten(FoodType foodType)
    {
        int idx = (int)foodType;
        if (idx < 0 || idx >= _data.foodEatenCounts.Length) return;
        _data.foodEatenCounts[idx]++;
        _sessionFoodCounts[idx]++;
        Save();
        CheckUnlockConditions();
    }

    private void CheckUnlockConditions()
    {
        foreach (var item in DataTable.AllItems)
        {
            if (!item.IsUnlockType) continue;
            if (IsUnlocked(item.Itemname)) continue;
            if (IsPurchased(item.Itemname)) continue;

            // 누적 또는 세션 둘 중 하나라도 조건을 충족하면 해금
            bool met = ConditionsMet(item, _data.foodEatenCounts)
                    || ConditionsMet(item, _sessionFoodCounts);

            if (met)
            {
                _data.unlockedItems.Add(item.Itemname);
                Save();
                Debug.Log($"[DataManager] 상점 해금 가능: {item.Itemname}");
                EventHub.Publish(item.Itemname);
            }
        }
    }

    private bool ConditionsMet(ItemData item, int[] counts)
    {
        foreach (var cond in item.UnlockConditions)
        {
            if (cond.foodType >= counts.Length || counts[cond.foodType] < cond.requiredCount)
                return false;
        }
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 사운드 볼륨
    // ─────────────────────────────────────────────────────────────────────────

    public float BgmVolume => _data.bgmVolume;
    public float SfxVolume => _data.sfxVolume;

    public void SetBgmVolume(float value) { _data.bgmVolume = Mathf.Clamp01(value); Save(); }
    public void SetSfxVolume(float value) { _data.sfxVolume = Mathf.Clamp01(value); Save(); }

    // ─────────────────────────────────────────────────────────────────────────
    // 유틸 (기존)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>디버그용 - 저장 경로를 반환합니다.</summary>
    public string GetSavePath() => SavePath;

#if UNITY_EDITOR
    /// <summary>에디터 전용 - 데이터를 초기화합니다.</summary>
    [ContextMenu("데이터 초기화 (에디터 전용)")]
    private void ResetData()
    {
        _data = new PlayerData();
        Save();
        Debug.Log("[DataManager] 데이터 초기화 완료.");
    }
#endif
}
