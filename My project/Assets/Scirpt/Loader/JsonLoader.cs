using UnityEngine;

public static class JsonLoader
{
    public static T LoadJson<T>(string path)
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(path);
        if(jsonAsset == null)
        {
            Debug.LogError($"[JsonLoader] JSON 파일을 찾을 수 없습니다: {path}");
            return default(T);
        }
        try
        {
            return JsonUtility.FromJson<T>(jsonAsset.text);
        }
        catch
        {
            Debug.LogError($"[JsonLoader] JSON 파싱 실패: {path}");
            return default(T);
        }

    }
}
