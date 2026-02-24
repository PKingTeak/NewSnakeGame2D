using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Food Setup")]
    public GameObject foodPrefab;
    public BoxCollider2D gridArea;
    public int foodCount = 3; // 화면에 유지할 먹이 개수

    private int _score = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (gridArea != null)
        {
            for (int i = 0; i < foodCount; i++)
            {
                SpawnFood();
            }
        }
    }

    public void SpawnFood()
    {
        if (gridArea == null || foodPrefab == null) return;

        Bounds bounds = gridArea.bounds;
        float padding = 1.5f;
        Vector3 spawnPos = Vector3.zero;
        bool isPosValid = false;
        int attempts = 0;

        // [핵심] 빈 자리를 찾을 때까지 반복 (최대 20번 시도)
        while (!isPosValid && attempts < 20)
        {
            float x = Mathf.Round(Random.Range(bounds.min.x + padding, bounds.max.x - padding));
            float y = Mathf.Round(Random.Range(bounds.min.y + padding, bounds.max.y - padding));
            spawnPos = new Vector3(x, y, 0);

            // 해당 좌표에 이미 콜라이더(먹이나 몸통)가 있는지 확인
            Collider2D hit = Physics2D.OverlapPoint(spawnPos);
            if (hit == null)
            {
                isPosValid = true;
            }
            attempts++;
        }

        GameObject obj = Instantiate(foodPrefab, spawnPos, Quaternion.identity);
        Food foodScript = obj.GetComponent<Food>();
        if (foodScript != null)
        {
            foodScript.SetType((FoodType)Random.Range(0, 3));
        }
    }

    public void AddScore(int amount)
    {
        _score += amount;
        Debug.Log($"<color=yellow>Score: {_score}</color>"); // 점수 로그 확인
    }
}