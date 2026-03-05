using UnityEngine;
using UnityEngine.Tilemaps; // Ÿ�ϸ� �ý��� ���

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Tilemap Setup")]
    public Tilemap groundTilemap; // 'Ÿ�� ���' ������Ʈ�� ���⿡ �巡���ϼ���.
    public LayerMask obstacleLayer; // ��(Wall)�� ������(Snake) ���̾ �����ϼ���.

    [Header("Food Setup")]
    public GameObject foodPrefab;
    public int foodCount = 3;

    private int _score = 0;
    private float time = 0.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (groundTilemap != null)
        {
            for (int i = 0; i < foodCount; i++)
            {
                SpawnFood();
            }
        }
    }

    private void Update()
    {
        time += Time.deltaTime;
        UIManager.Instance.UpdateTime(time);
    }

    public void SpawnFood()
    {
        if (groundTilemap == null || foodPrefab == null) return;

        BoundsInt bounds = groundTilemap.cellBounds;
        Vector3 spawnPos = Vector3.zero;
        bool isPosValid = false;
        int attempts = 0;

        while (!isPosValid && attempts < 50)
        {
            // 1. Ÿ�� ��ǥ�� �������� ���� ��ǥ ����
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int cellPos = new Vector3Int(x, y, 0);

            if (groundTilemap.HasTile(cellPos))
            {
                spawnPos = groundTilemap.GetCellCenterWorld(cellPos);

                // 2. �ش� ��ġ�� ������ �ִ��� Ȯ��
                Collider2D hit = Physics2D.OverlapPoint(spawnPos);

                // [����] �ƹ��͵� ���ų�(null), 
                // Ȥ�� ��ֹ� ���̾ �ɸ��� �����鼭 �±װ� "Food"�� �ƴ� ��쿡�� ��ȿ
                if (hit == null)
                {
                    isPosValid = true;
                }
                else if (hit.CompareTag("Food") || hit.CompareTag("Player") || hit.CompareTag("Body"))
                {
                    // �̹� ���̰� �ְų�, ������(�Ӹ�/����)�� �ִ� ĭ�̸� �ٽ� �õ�
                    isPosValid = false;
                }
            }
            attempts++;
        }

        if (isPosValid)
        {
            GameObject obj = Instantiate(foodPrefab, spawnPos, Quaternion.identity);
            Food foodScript = obj.GetComponent<Food>();
            if (foodScript != null)
            {
                foodScript.SetType((FoodType)Random.Range(0, 3));
            }
        }
    }


    public void Reset()
    {
        _score = 0;
        time = 0.0f;
    }

    public void AddScore(int amount)
    {
        _score += amount;
        DataManager.Instance.AddGold(amount); // 점수와 동일한 골드 지급
        UIManager.Instance.UpdateScore(_score);
        Debug.Log($"<color=yellow>Score: {_score}</color>");
    }

    public void OnGameOver()
    {
        DataManager.Instance.TryUpdateHighScore(_score);
    }
}