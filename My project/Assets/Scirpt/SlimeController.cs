using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite headSprite;
    public Sprite[] babySlimeSprites;

    [Header("Settings")]
    public float moveInterval = 0.2f;
    public Transform babyPrefab; // 인스펙터에서 연결 필수!

    private List<Transform> _segments = new List<Transform>();
    private List<FoodType> _babyTypes = new List<FoodType>();

    private Vector2 _direction = Vector2.right;
    private Vector2 _inputDirection = Vector2.right;
    private float _timer;
    private SpriteRenderer _sr;
    private Collider2D _headCollider; // 머리 충돌체 저장

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _headCollider = GetComponent<Collider2D>(); // 머리 콜라이더 가져오기
    }

    private void Start()
    {
        Time.timeScale = 1f;
        _segments.Clear();
        _babyTypes.Clear();

        _segments.Add(this.transform);
        _babyTypes.Add((FoodType)(-1));

        if (headSprite != null) _sr.sprite = headSprite;
    }

    private void Update()
    {
        // 방향 전환 (반대 방향 금지)
        if (Input.GetKeyDown(KeyCode.UpArrow) && _direction != Vector2.down) _inputDirection = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && _direction != Vector2.up) _inputDirection = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && _direction != Vector2.right) _inputDirection = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && _direction != Vector2.left) _inputDirection = Vector2.right;

        _timer += Time.deltaTime;
        if (_timer >= moveInterval)
        {
            _timer = 0f;
            _direction = _inputDirection;
            Move();
        }
    }

    private void Move()
    {
        // 1. 뒤에서부터 앞 위치 따라가기
        for (int i = _segments.Count - 1; i > 0; i--)
        {
            _segments[i].position = _segments[i - 1].position;
        }

        // 2. 머리 이동
        this.transform.position += (Vector3)_direction;

        // 좌우 반전
        if (_direction == Vector2.right) _sr.flipX = true;
        else if (_direction == Vector2.left) _sr.flipX = false;
    }

    private void Grow(FoodType type)
    {
        if (babyPrefab == null) return;

        // 현재 꼬리의 위치를 미리 저장
        Vector3 spawnPos = _segments[_segments.Count - 1].position;

        // 아기 슬라임 생성
        Transform newBaby = Instantiate(babyPrefab, spawnPos, Quaternion.identity);

        // [핵심 해결] 머리와 새로 생성된 아기 슬라임의 물리 충돌을 무시하도록 설정
        Collider2D babyCollider = newBaby.GetComponent<Collider2D>();
        if (babyCollider != null && _headCollider != null)
        {
            Physics2D.IgnoreCollision(_headCollider, babyCollider);
        }

        // 속성 이미지 설정
        SpriteRenderer babySR = newBaby.GetComponent<SpriteRenderer>();
        if (babySR != null) babySR.sprite = babySlimeSprites[(int)type];

        _segments.Add(newBaby);
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
        else if (other.CompareTag("Wall") || other.CompareTag("Body"))
        {
            // 벽에 부딪히거나, 아주 멀리 있는 몸통(IgnoreCollision이 안 된 부분)에 닿으면 게임 종료
            Time.timeScale = 0;
            Debug.Log("게임 오버!");
        }
    }
}