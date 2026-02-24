using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite headSprite;
    public Sprite[] babySlimeSprites;

    [Header("Settings")]
    public float moveInterval = 0.2f;
    public Transform babyPrefab;

    private List<Transform> _segments = new List<Transform>();
    private List<FoodType> _babyTypes = new List<FoodType>();
    private List<Vector3> _targetPositions = new List<Vector3>();

    private Vector2 _direction = Vector2.right;
    private Vector2 _inputDirection = Vector2.right;
    private float _timer;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Time.timeScale = 1f;
        _segments.Clear();
        _babyTypes.Clear();
        _targetPositions.Clear();

        _segments.Add(this.transform);
        _targetPositions.Add(this.transform.position);
        _babyTypes.Add((FoodType)(-1));

        if (headSprite != null) _sr.sprite = headSprite;
    }

    private void Update()
    {
        HandleInput();

        _timer += Time.deltaTime;
        if (_timer >= moveInterval)
        {
            _timer = 0f;
            _direction = _inputDirection;
            UpdateGridLogic();
        }

        SmoothMove();
    }

    private void HandleInput()
    {
        // 180도 급회전 방지 로직 포함
        if (Input.GetKeyDown(KeyCode.UpArrow) && _direction != Vector2.down) _inputDirection = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && _direction != Vector2.up) _inputDirection = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && _direction != Vector2.right) _inputDirection = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && _direction != Vector2.left) _inputDirection = Vector2.right;
    }

    private void UpdateGridLogic()
    {
        Vector3 nextPos = _targetPositions[0] + (Vector3)_direction;

        // [추가] 1. 경계선 밖으로 나갔는지 체크
        if (!IsInsideBoundary(nextPos))
        {
            GameOver("경계선 밖으로 나감!");
            return;
        }

        // [추가] 2. 자기 몸통에 부딪혔는지 체크 (좌표 기반)
        // i=1부터 시작하여 머리 다음 마디부터 검사합니다.
        for (int i = 1; i < _targetPositions.Count; i++)
        {
            // 다음 이동할 위치에 이미 몸통 마디가 있다면 게임 오버
            if (Vector3.Distance(nextPos, _targetPositions[i]) < 0.1f)
            {
                GameOver("자신의 몸에 부딪힘!");
                return;
            }
        }

        // 3. 마디 위치 갱신
        for (int i = _segments.Count - 1; i > 0; i--)
        {
            _targetPositions[i] = _targetPositions[i - 1];
        }

        _targetPositions[0] = nextPos;

        if (_direction == Vector2.right) _sr.flipX = true;
        else if (_direction == Vector2.left) _sr.flipX = false;
    }

    private bool IsInsideBoundary(Vector3 pos)
    {
        if (GameManager.Instance.gridArea == null) return true;
        return GameManager.Instance.gridArea.bounds.Contains(pos);
    }

    private void SmoothMove()
    {
        float speed = 1f / moveInterval;
        for (int i = 0; i < _segments.Count; i++)
        {
            _segments[i].position = Vector3.MoveTowards(_segments[i].position, _targetPositions[i], speed * Time.deltaTime);
        }
    }

    private void Grow(FoodType type)
    {
        if (babyPrefab == null) return;

        // 마지막 마디의 현재 위치에 생성
        Vector3 spawnPos = _segments[_segments.Count - 1].position;
        Transform newBaby = Instantiate(babyPrefab, spawnPos, Quaternion.identity);

        SpriteRenderer babySR = newBaby.GetComponent<SpriteRenderer>();
        if (babySR != null) babySR.sprite = babySlimeSprites[(int)type];

        _segments.Add(newBaby);
        _targetPositions.Add(spawnPos);
        _babyTypes.Add(type);

        CheckCombo();
    }

    private void CheckCombo()
    {
        if (_segments.Count < 4) return;
        int last = _babyTypes.Count - 1;
        if (_babyTypes[last] == _babyTypes[last - 1] && _babyTypes[last] == _babyTypes[last - 2])
        {
            GameManager.Instance.AddScore(50);
            for (int i = 0; i < 3; i++)
            {
                int targetIndex = _segments.Count - 1;
                Destroy(_segments[targetIndex].gameObject);
                _segments.RemoveAt(targetIndex);
                _targetPositions.RemoveAt(targetIndex);
                _babyTypes.RemoveAt(targetIndex);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            Food food = other.GetComponent<Food>();
            if (food != null)
            {
                Grow(food.foodType);
                GameManager.Instance.AddScore(10);
                Destroy(other.gameObject);
                GameManager.Instance.SpawnFood();
            }
        }
        // 이제 'Body' 태그 충돌은 UpdateGridLogic에서 좌표로 처리하므로 무시해도 안전합니다.
    }

    private void GameOver(string reason)
    {
        Time.timeScale = 0;
        Debug.Log($"<color=red>GAME OVER!</color> {reason}");
    }
}