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
    private Collider2D _headCollider;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _headCollider = GetComponent<Collider2D>();
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
        if (Input.GetKeyDown(KeyCode.UpArrow) && _direction != Vector2.down) _inputDirection = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && _direction != Vector2.up) _inputDirection = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && _direction != Vector2.right) _inputDirection = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && _direction != Vector2.left) _inputDirection = Vector2.right;
    }

    private void UpdateGridLogic()
    {
        // 1. 다음 이동할 '예정' 위치를 미리 계산합니다.
        Vector3 nextPos = _targetPositions[0] + (Vector3)_direction;

        // 2. [핵심] 다음 위치가 맵 경계선 밖인지 체크합니다.
        if (!IsInsideBoundary(nextPos))
        {
            GameOver();
            return;
        }

        // 3. 맵 안이라면 마디들의 위치를 갱신합니다.
        for (int i = _segments.Count - 1; i > 0; i--)
        {
            _targetPositions[i] = _targetPositions[i - 1];
        }

        _targetPositions[0] = nextPos;

        if (_direction == Vector2.right) _sr.flipX = true;
        else if (_direction == Vector2.left) _sr.flipX = false;
    }

    // [핵심 함수] 현재 좌표가 GameManager의 gridArea 범위 안에 있는지 확인
    private bool IsInsideBoundary(Vector3 pos)
    {
        if (GameManager.Instance.gridArea == null)
        {
            Debug.LogError("GameManager의 Grid Area가 비어있습니다!");
            return true;
        }

        // 배경 BoxCollider2D의 영역(Bounds) 안에 좌표가 있는지 수학적으로 판단
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

        Vector3 spawnPos = _segments[_segments.Count - 1].position;
        Transform newBaby = Instantiate(babyPrefab, spawnPos, Quaternion.identity);

        Collider2D babyCollider = newBaby.GetComponent<Collider2D>();
        if (babyCollider != null && _headCollider != null)
        {
            Physics2D.IgnoreCollision(_headCollider, babyCollider);
        }

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
            Grow(food.foodType);
            GameManager.Instance.AddScore(10);
            Destroy(other.gameObject);
            GameManager.Instance.SpawnFood();
        }
        else if (other.CompareTag("Body")) // 이제 벽(Wall) 체크는 필요 없습니다.
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Time.timeScale = 0;
        Debug.Log("<color=red>GAME OVER!</color> 경계선 밖으로 나갔거나 몸에 닿았습니다.");
    }
}