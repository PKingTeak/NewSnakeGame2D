using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [System.Serializable]
    public struct SnakeSkin // 머리와 기본 꼬리를 세트로 묶는 구조체
    {
        public string skinName;
        public Sprite headSprite;
        public Sprite defaultBodySprite; // 무속성 상태의 몸통
        public float scoreBonus;         // 이 스킨의 점수 보너스
    }

    [Header("Skin Shop Data")]
    public SnakeSkin[] skinInventory; // 구매 가능한 스킨 리스트
    public int currentSkinIndex = 0;

    [Header("Element Body Sprites")]
    // 0:불, 1:풀, 2:물 (먹이를 먹었을 때 변하는 속성 몸통들)
    public Sprite[] elementBodySprites;

    [Header("Settings")]
    public float moveInterval = 0.1f;
    public Transform segmentPrefab;
    public int initialSize = 3;

    private List<Transform> _segments = new List<Transform>();
    private Vector2 _direction = Vector2.right;
    private float _timer;
    private SpriteRenderer _headRenderer;
    private FoodType _lastEatenType;
    private int _comboCount = 0;

    private void Awake() => _headRenderer = GetComponent<SpriteRenderer>();

    private void Start()
    {
        _segments.Add(this.transform);
        ApplySkin(currentSkinIndex); // 시작할 때 선택된 스킨 적용

        // 초기 꼬리 생성 (현재 스킨의 기본 몸통 사용)
        for (int i = 1; i < initialSize; i++)
        {
            Grow(skinInventory[currentSkinIndex].defaultBodySprite);
        }
    }

    // [핵심] 머리와 기본 꼬리 세트를 적용하는 함수
    public void ApplySkin(int index)
    {
        if (index < 0 || index >= skinInventory.Length) return;

        currentSkinIndex = index;
        _headRenderer.sprite = skinInventory[index].headSprite;

        // 기존에 달려있던 무속성 꼬리들도 새 스킨으로 갈아끼우고 싶다면 
        // 리스트를 돌며 이미지를 교체하는 로직을 추가할 수 있습니다.
    }

    private void Update()
    {
        HandleInput();
        _timer += Time.deltaTime;
        if (_timer >= moveInterval) { Move(); _timer = 0f; }
    }

    // ... (Move, HandleInput 로직은 동일) ...

    private void Grow(Sprite bodySprite)
    {
        Transform segment = Instantiate(segmentPrefab);
        segment.position = _segments[_segments.Count - 1].position;
        segment.GetComponent<SpriteRenderer>().sprite = bodySprite;
        _segments.Add(segment);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            Food food = other.GetComponent<Food>();

            // 먹이 속성에 맞는 몸통 생성 (0:불, 1:풀, 2:물)
            Grow(elementBodySprites[(int)food.foodType]);

            ProcessScoreAndCombo(food);
            Destroy(other.gameObject);
            GameManager.Instance.SpawnFood();
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Body"))
        {
            Time.timeScale = 0;
        }
    }

    private void ProcessScoreAndCombo(Food food)
    {
        // 스킨 보너스 + 기본 점수
        int score = Mathf.RoundToInt(10 + skinInventory[currentSkinIndex].scoreBonus);
        GameManager.Instance.AddScore(score);

        if (_comboCount == 0 || food.foodType == _lastEatenType) _comboCount++;
        else _comboCount = 1;

        _lastEatenType = food.foodType;

        if (_comboCount >= 3)
        {
            Shrink(3);
            GameManager.Instance.AddScore(50);
            _comboCount = 0;
        }
    }

    private void Shrink(int amount)
    {
        int removeCount = Mathf.Min(amount, _segments.Count - 1);
        for (int i = 0; i < removeCount; i++)
        {
            int lastIndex = _segments.Count - 1;
            Destroy(_segments[lastIndex].gameObject);
            _segments.RemoveAt(lastIndex);
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && _direction != Vector2.down) _direction = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && _direction != Vector2.up) _direction = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && _direction != Vector2.right) _direction = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && _direction != Vector2.left) _direction = Vector2.right;
    }

    private void Move()
    {
        for (int i = _segments.Count - 1; i > 0; i--)
        {
            _segments[i].position = _segments[i - 1].position;
        }

        this.transform.position = new Vector3(
            Mathf.Round(this.transform.position.x + _direction.x),
            Mathf.Round(this.transform.position.y + _direction.y),
            0.0f
        );
    }
}