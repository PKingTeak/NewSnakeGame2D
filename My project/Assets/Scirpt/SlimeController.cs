using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

[System.Serializable]
public struct SlimeSkinData
{
    public string itemId;
    public Sprite sprite;
}

public class SlimeController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite headSprite;
    public Sprite[] babySlimeSprites;
    
    [Header("Skin Overrides (상점 연동)")]
    [SerializeField] private SlimeSkinData[] skinEntries;

    [Header("Settings")]
    [SerializeField] private float moveInterval = 0.3f;
    [SerializeField] float minmoveInterval = 0.1f;
    [SerializeField] float moveSpeed = 0.0002f;
    public Transform babyPrefab;

    private List<Transform> _segments = new List<Transform>();
    private List<FoodType> _babyTypes = new List<FoodType>();
    private List<Vector3> _targetPositions = new List<Vector3>();

    //터치 입력
    private Vector2 startTouchPosition;
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

        // [�ٽ� �߰�] ���� ��ġ�� Ÿ�ϸ��� �׸��� �߾ӿ� �� ����
        if (GameManager.Instance != null && GameManager.Instance.groundTilemap != null)
        {
            Vector3Int cellPos = GameManager.Instance.groundTilemap.WorldToCell(transform.position);
            transform.position = GameManager.Instance.groundTilemap.GetCellCenterWorld(cellPos);
        }

        _segments.Add(this.transform);
        _targetPositions.Add(this.transform.position); // ���� ���ĵ� ��ǥ�� ��
        _babyTypes.Add((FoodType)(-1));

        if (headSprite != null) _sr.sprite = headSprite;
        ApplySelectedSkin();
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
        /*
        if (Input.GetKeyDown(KeyCode.UpArrow) && _direction != Vector2.down) _inputDirection = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && _direction != Vector2.up) _inputDirection = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && _direction != Vector2.right) _inputDirection = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && _direction != Vector2.left) _inputDirection = Vector2.right;
        */

        //모바일 터치화면
        if(Input.touchCount >0)
        {
            Touch touch = Input.GetTouch(0);
            
            if(touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                Vector2 endTouchPosition = touch.position;
                Vector2 delta = endTouchPosition - startTouchPosition;
                if(delta.magnitude <30)
                {
                    return; //너무 작은 반지름은 터치 실수 유발
                }
                if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    if(delta.x>0 && _direction != Vector2.left)
                    {
                        _inputDirection = Vector2.right;
                    }
                    else if(delta.x<0 && _direction != Vector2.right)
                    {
                        _inputDirection = Vector2.left;
                    }
                }
                else
                {
                    if(delta.y > 0 && _direction != Vector2.down)
                    {
                        _inputDirection = Vector2.up;
                    }
                    else if(delta.y < 0 && _direction != Vector2.up)
                    {
                        _inputDirection = Vector2.down;
                    }
                }
            }
            

        }
    }

    private void UpdateGridLogic()
    {
        // ���� �̵��� ���� ��ǥ ��� (Ÿ�� �� ĭ �̵�)
        Vector3 nextPos = _targetPositions[0] + (Vector3)_direction;

        // 1. Ÿ�ϸ� ��� �� ��ֹ� üũ
        if (!IsSafePos(nextPos))
        {
            GameOver("��� ���̰ų� ���� �ε���!");
            return;
        }

        // 2. �ڱ� ���� �浹 üũ
        for (int i = 1; i < _targetPositions.Count; i++)
        {
            if (Vector3.Distance(nextPos, _targetPositions[i]) < 0.1f)
            {
                GameOver("�ڽ��� ���� �ε���!");
                return;
            }
        }

        // 3. ���� ��ġ ���� (�ڿ������� ������ ����)
        for (int i = _segments.Count - 1; i > 0; i--)
        {
            _targetPositions[i] = _targetPositions[i - 1];
        }

        _targetPositions[0] = nextPos;

        // �ð��� ���� ��ȯ
        if (_direction == Vector2.right) _sr.flipX = true;
        else if (_direction == Vector2.left) _sr.flipX = false;
    }

    // Ÿ�ϸʰ� ���̾ Ȱ���� ���� �˻�
    private bool IsSafePos(Vector3 pos)
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.groundTilemap == null) return true;

        // Ÿ�� ��ǥ�� ��ȯ�Ͽ� �ش� ��ġ�� �ٴ� Ÿ���� �ִ��� Ȯ��
        Vector3Int cellPos = gm.groundTilemap.WorldToCell(pos);
        if (!gm.groundTilemap.HasTile(cellPos)) return false;

        // ��ֹ� ���̾�(Wall ��)�� �浹�ϴ��� Ȯ��
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
        GameManager.Instance.OnGameOver();
    }

    private void CalSpeed()
    {
        moveInterval = Mathf.Max(minmoveInterval, moveInterval - moveSpeed); //��ū ������ �ִ� min����

    }

    /// <summary>상점에서 선택된 슬라임 헤드 스킨을 적용.</summary>
    private void ApplySelectedSkin()
    {
        if (skinEntries == null || skinEntries.Length == 0) return;

        string selectedId = ShopDataManager.SelectedHeadId;
        foreach (var entry in skinEntries)
        {
            if (entry.itemId == selectedId && entry.sprite != null)
            {
                _sr.sprite = entry.sprite;
                return;
            }
        }
    }
}