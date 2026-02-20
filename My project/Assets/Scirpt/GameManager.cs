using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Food Setup")]
    public GameObject foodPrefab; // 유니티 인스펙터에서 Food 프리팹 하나만 연결하세요.

    public BoxCollider2D gridArea; // 먹이가 생성될 영역 (배경)

    private int _score = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (gridArea != null) SpawnFood();
        else Debug.LogError("Grid Area가 설정되지 않았습니다!");
    }

    public void SpawnFood()
    {
        if (gridArea == null || foodPrefab == null) return;

        Bounds bounds = gridArea.bounds;

        // 맵 테두리 밖으로 나가지 않도록 1칸 정도의 여백(padding)을 줍니다.
        float padding = 1.0f;

        // 랜덤 위치 계산 (여백 반영)
        float x = Mathf.Round(Random.Range(bounds.min.x + padding, bounds.max.x - padding));
        float y = Mathf.Round(Random.Range(bounds.min.y + padding, bounds.max.y - padding));

        // 하나의 프리팹만 생성
        GameObject obj = Instantiate(foodPrefab, new Vector3(x, y, 0), Quaternion.identity);

        // 생성된 먹이의 스크립트를 가져와서 랜덤 속성(불, 풀, 물) 부여
        Food foodScript = obj.GetComponent<Food>();
        if (foodScript != null)
        {
            // 0:불, 1:풀, 2:물 중 하나로 랜덤 설정
            FoodType randomType = (FoodType)Random.Range(0, 3);
            foodScript.SetType(randomType);
        }
    }

    public void AddScore(int amount)
    {
        _score += amount;
        Debug.Log($"Current Score: {_score}");
    }
}