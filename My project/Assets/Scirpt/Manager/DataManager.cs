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
        public string selectedHeadId = "slime_default";
        public string selectedMapId  = "map_default";
    }

    private PlayerData _data;

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
        Load();
    }

    public void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            _data = JsonUtility.FromJson<PlayerData>(json);
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
    // 유틸
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
