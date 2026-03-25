using System.IO;
using UnityEditor;
using UnityEngine;

public static class DataManagerEditor
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "playerdata.json");

    [MenuItem("Tool/Player Data/Reset Player Data")]
    static void ResetPlayerData()
    {
        if (!EditorUtility.DisplayDialog("데이터 초기화", "playerdata.json을 초기화하시겠습니까?", "초기화", "취소"))
            return;

        File.WriteAllText(SavePath, JsonUtility.ToJson(new DataManager.PlayerData(), prettyPrint: true));
        Debug.Log($"[DataManagerEditor] 데이터 초기화 완료: {SavePath}");
    }

    [MenuItem("Tool/Player Data/Delete Save File")]
    static void DeleteSaveFile()
    {
        if (!EditorUtility.DisplayDialog("파일 삭제", "playerdata.json을 삭제하시겠습니까?", "삭제", "취소"))
            return;

        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log($"[DataManagerEditor] 파일 삭제 완료: {SavePath}");
        }
        else
        {
            Debug.LogWarning("[DataManagerEditor] 저장 파일이 없습니다.");
        }
    }

    [MenuItem("Tool/Player Data/Open Save Folder")]
    static void OpenSaveFolder()
    {
        EditorUtility.RevealInFinder(SavePath);
    }

    [MenuItem("Tool/Player Data/Get Gold +1000")]
    static void AddGold()
    {
        DataManager.Instance.Data.gold += 1000;
        DataManager.Instance.Save();
        Debug.Log("[DataManagerEditor] 골드 +1000");

    }
}
