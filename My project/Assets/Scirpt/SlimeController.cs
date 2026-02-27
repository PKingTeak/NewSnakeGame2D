using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite headSprite;
    public Sprite[] babySlimeSprites;

    [Header("Settings")]
    public float moveInterval = 0.2f;
    public float moveSpeed = 0.02f;
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

        // [핵심 추가] 시작 위치를 타일맵의 그리드 중앙에 딱 맞춤
        if (GameManager.Instance != null && GameManager.Instance.groundTilemap != null)
        {
            Vector3Int cellPos = GameManager.Instance.groundTilemap.WorldToCell(transform.position);
            transform.position = GameManager.Instance.groundTilemap.GetCellCenterWorld(cellPos);
        }

        _segments.Add(this.transform);
        _targetPositions.Add(this.transform.position); // 이제 정렬된 좌표가 들어감
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
        if (Input.GetKeyDown(KeyCode.UpArrow) && _direction != Vector2.down) _inputDirection = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && _direction != Vector2.up) _inputDirection = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && _direction != Vector2.right) _inputDirection = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && _direction != Vector2.left) _inputDirection = Vector2.right;
    }

    private void UpdateGridLogic()
    {
        // 다음 이동할 월드 좌표 계산 (타일 한 칸 이동)
        Vector3 nextPos = _targetPositions[0] + (Vector3)_direction;

        // 1. 타일맵 경계 및 장애물 체크
        if (!IsSafePos(nextPos))
        {
            GameOver("경계 밖이거나 벽에 부딪힘!");
            return;
        }

        // 2. 자기 몸통 충돌 체크
        for (int i = 1; i < _targetPositions.Count; i++)
        {
            if (Vector3.Distance(nextPos, _targetPositions[i]) < 0.1f)
            {
                GameOver("자신의 몸에 부딪힘!");
                return;
            }
        }

        // 3. 마디 위치 갱신 (뒤에서부터 앞으로 전달)
        for (int i = _segments.Count - 1; i > 0; i--)
        {
            _targetPositions[i] = _targetPositions[i - 1];
        }

        _targetPositions[0] = nextPos;

        // 시각적 방향 전환
        if (_direction == Vector2.right) _sr.flipX = true;
        else if (_direction == Vector2.left) _sr.flipX = false;
    }

    // 타일맵과 레이어를 활용한 안전 검사
    private bool IsSafePos(Vector3 pos)
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.groundTilemap == null) return true;

        // 타일 좌표로 변환하여 해당 위치에 바닥 타일이 있는지 확인
        Vector3Int cellPos = gm.groundTilemap.WorldToCell(pos);
        if (!gm.groundTilemap.HasTile(cellPos)) return false;

        // 장애물 레이어(Wall 등)와 충돌하는지 확인
        Collider2D hit = Physics2D.OverlapPoint(pos, gm.obstacleLayer);
        if (hit != null) return false;

        return true;
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

        Vector3 spawnPos = _segments[_segments.Count - 1].position;
        Transform newBaby = Instantiate(babyPrefab, spawnPos, Quaternion.identity);

        SpriteRenderer babySR = newBaby.GetComponent<SpriteRenderer>();
        if (babySR != null) babySR.sprite = babySlimeSprites[(int)type];

        _segments.Add(newBaby);
        _targetPositions.Add(spawnPos);
        _babyTypes.Add(type);

        CalSpeed();
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
    }

    private void GameOver(string reason)
    {
        Time.timeScale = 0;
        Debug.Log($"<color=red>GAME OVER!</color> {reason}");
    }

    private void CalSpeed()
    {
        moveInterval -= moveSpeed;
        if (moveInterval <= 0.1)
        {
            moveInterval -= Mathf.Clamp01(moveSpeed);
        }

    }
}