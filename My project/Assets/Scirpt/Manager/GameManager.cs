using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps; // 타일맵 시스템 사용

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Tilemap Setup")]
    public Tilemap groundTilemap; // '타일 맵' 컴포넌트를 여기에 드래그하세요.
    public LayerMask obstacleLayer; // 장벽(Wall)과 뱀(Snake) 레이어를 설정하세요.

    [Header("Food Setup")]
    public GameObject foodPrefab;
    public int foodCount = 3;

    private int _score = 0;
    private float time = 0.0f;
    [Header("InGamePlayer")]
    [SerializeField] private SlimeController slimeController; //최초 실행할때 한번 그냥 탐색

    public SlimeController SlimeController{get{return slimeController;} private set { slimeController = value; }}


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        DataManager.Instance.ResetSessionCounts();
        SoundManager.Instance?.PlayBgm(BgmType.Game);
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
            // 1. 타일 좌표를 랜덤으로 선택된 좌표 생성
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int cellPos = new Vector3Int(x, y, 0);

            if (groundTilemap.HasTile(cellPos))
            {
                spawnPos = groundTilemap.GetCellCenterWorld(cellPos);

                // 2. 해당 위치에 무언가 있는지 확인
                Collider2D hit = Physics2D.OverlapPoint(spawnPos);

                // [조건] 아무것도 없거나(null),
                // 혹은 장벽 레이어에 걸리면서 태그가 "Food"가 아닌 경우에도 유효
                if (hit == null)
                {
                    isPosValid = true;
                }
                else if (hit.CompareTag("Food") || hit.CompareTag("Player") || hit.CompareTag("Body"))
                {
                    // 이미 뭔가 있거나, 음식(머리/몸통)이 있는 칸이면 다시 시도
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
        UIManager.Instance.UpdateScore(_score);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void OnGameOver()
    {
        SoundManager.Instance?.PlaySfx(SfxType.GameOver);
        DataManager.Instance.TryUpdateHighScore(_score);
        EventHub.Publish(_score); //점수 이벤트 넣어주기;
        UIManager.Instance.ShowGameOverPopup(_score, time);
    }

   
}
