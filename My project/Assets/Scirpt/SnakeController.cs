using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("Settings")]
    public float moveInterval = 0.1f; // 이동 속도
    public Transform segmentPrefab;   // 꼬리 프리팹 (Player와 같은 모양)
    public int initialSize = 3;

    [Header("Game State")]
    private Vector2 _direction = Vector2.right;
    private List<Transform> _segments = new List<Transform>();
    private float _timer;

    // 콤보 로직 변수
    private FoodType _lastEatenType;
    private int _comboCount = 0;

    private void Start()
    {
        _segments.Add(this.transform); // 머리를 리스트 첫 번째로 추가

        // 초기 꼬리 생성
        for (int i = 1; i < initialSize; i++)
        {
            Grow();
        }
    }

    private void Update()
    {
        HandleInput();

        _timer += Time.deltaTime;
        if (_timer >= moveInterval)
        {
            Move();
            _timer = 0f;
        }
    }

    private void HandleInput()
    {
        // 180도 회전 방지 로직 포함
        if (Input.GetKeyDown(KeyCode.UpArrow) && _direction != Vector2.down) _direction = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && _direction != Vector2.up) _direction = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && _direction != Vector2.right) _direction = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && _direction != Vector2.left) _direction = Vector2.right;
    }

    private void Move()
    {
        // 꼬리 이동 (뒤에서부터 앞의 위치로 이동)
        for (int i = _segments.Count - 1; i > 0; i--)
        {
            _segments[i].position = _segments[i - 1].position;
        }

        // 머리 이동
        this.transform.position = new Vector3(
            Mathf.Round(this.transform.position.x + _direction.x),
            Mathf.Round(this.transform.position.y + _direction.y),
            0.0f
        );
    }

    private void Grow()
    {
        Transform segment = Instantiate(segmentPrefab);
        segment.position = _segments[_segments.Count - 1].position; // 마지막 꼬리 위치에 생성
        _segments.Add(segment);
    }

    // 꼬리 줄이기 (보너스 효과)
    private void Shrink(int amount)
    {
        // 최소 머리는 남겨야 하므로 1보다는 커야 함
        int removeCount = Mathf.Min(amount, _segments.Count - 1);

        for (int i = 0; i < removeCount; i++)
        {
            int lastIndex = _segments.Count - 1;
            Destroy(_segments[lastIndex].gameObject);
            _segments.RemoveAt(lastIndex);
        }

        Debug.Log($"<color=yellow>BONUS! Shrinking tail by {removeCount}!</color>");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            Food food = other.GetComponent<Food>();
            ProcessFoodEat(food);
            Destroy(other.gameObject); // 먹은 음식 제거
            GameManager.Instance.SpawnFood(); // 새 음식 생성
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Body"))
        {
            // 게임 오버 로직 (여기서는 씬 리로드 대신 로그만 출력)
            Debug.Log("Game Over!");
            Time.timeScale = 0; // 일시 정지
        }
    }

    // 핵심: 같은 종류 3개 먹었는지 판단하는 로직
    private void ProcessFoodEat(Food food)
    {
        // 1. 일단 꼬리는 무조건 늘어남 (기본 규칙)
        Grow();
        GameManager.Instance.AddScore(10); // 기본 점수

        // 2. 콤보 체크
        if (_comboCount == 0) // 첫 먹이
        {
            _lastEatenType = food.foodType;
            _comboCount = 1;
        }
        else
        {
            if (food.foodType == _lastEatenType)
            {
                _comboCount++;
            }
            else
            {
                // 다른 종류를 먹으면 콤보 초기화 (현재 먹은걸 1로)
                _lastEatenType = food.foodType;
                _comboCount = 1;
            }
        }

        // 3. 3연속 달성 시
        if (_comboCount >= 3)
        {
            Shrink(3); // 꼬리 3개 줄이기
            GameManager.Instance.AddScore(50); // 보너스 점수
            _comboCount = 0; // 콤보 리셋
        }
    }
}