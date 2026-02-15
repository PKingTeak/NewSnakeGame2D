using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject foodPrefab; // Food 스크립트가 붙은 프리팹
    public BoxCollider2D gridArea; // 먹이가 생성될 영역 (배경)

    private int _score = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnFood();
    }

    public void SpawnFood()
    {
        Bounds bounds = gridArea.bounds;

        // 그리드에 맞춰 랜덤 위치 계산
        float x = Mathf.Round(Random.Range(bounds.min.x, bounds.max.x));
        float y = Mathf.Round(Random.Range(bounds.min.y, bounds.max.y));

        GameObject obj = Instantiate(foodPrefab, new Vector3(x, y, 0), Quaternion.identity);

        // 랜덤 타입 부여 (3가지)
        Food foodScript = obj.GetComponent<Food>();
        FoodType randomType = (FoodType)Random.Range(0, 3); // 0, 1, 2 중 하나
        foodScript.SetType(randomType);
    }

    public void AddScore(int amount)
    {
        _score += amount;
        Debug.Log($"Current Score: {_score}");
        // 여기에 UI 업데이트 코드를 추가하면 됩니다.
    }
}