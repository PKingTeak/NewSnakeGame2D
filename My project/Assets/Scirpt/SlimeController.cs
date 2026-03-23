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
    [SerializeField] private float moveInterval = 0.08f;
    [SerializeField] float minmoveInterval = 0.05f;
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

        // [시작 시 초기화] 현재 위치를 타일맵의 그리드 중앙에 맞추기
        if (GameManager.Instance != null && GameManager.Instance.groundTilemap != null)
        {
            Vector3Int cellPos = GameManager.Instance.groundTilemap.WorldToCell(transform.position);
            transform.position = GameManager.Instance.groundTilemap.GetCellCenterWorld(cellPos);
        }

        _segments.Add(this.transform);
        _targetPositions.Add(this.transform.position); // 현재 타겟된 좌표의 초기값 추가
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
        #if UNITY_EDITOR || KEyBOARD_INPUT
        
        if (Input.GetKeyDown(KeyCode.UpArrow) && _direction != Vector2.down) _inputDirection = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && _direction != Vector2.up) _inputDirection = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && _direction != Vector2.right) _inputDirection = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && _direction != Vector2.left) _inputDirection = Vector2.right;
        #endif

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
        // 다음 이동할 목표 좌표 계산 (타일 한 칸 이동)
        Vector3 nextPos = _targetPositions[0] + (Vector3)_direction;

        // 1. 타일맵 범위 및 장벽 체크
        if (!IsSafePos(nextPos))
        {
            GameOver("범위를 벗어났거나 장벽에 닿았습니다!");
            return;
        }

        // 2. 자기 몸 충돌 체크
        for (int i = 1; i < _targetPositions.Count; i++)
        {
            if (Vector3.Distance(nextPos, _targetPositions[i]) < 0.1f)
            {
                GameOver("자신의 몸에 충돌했습니다!");
                return;
            }
        }

        // 3. 위치 이동 (뒤에서 앞으로 순서대로 이동)
        for (int i = _segments.Count - 1; i > 0; i--)
        {
            _targetPositions[i] = _targetPositions[i - 1];
        }

        _targetPositions[0] = nextPos;

        // 방향에 따른 좌우 반전
        if (_direction == Vector2.right) _sr.flipX = true;
        else if (_direction == Vector2.left) _sr.flipX = false;
    }

    // 타일맵과 레이어 활성화 여부 검사
    private bool IsSafePos(Vector3 pos)
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.groundTilemap == null) return true;

        // 타일 좌표로 변환하여 해당 위치에 바닥 타일이 있는지 확인
        Vector3Int cellPos = gm.groundTilemap.WorldToCell(pos);
        if (!gm.groundTilemap.HasTile(cellPos)) return false;

        // 장벽 레이어(Wall 등)와 충돌하는지 확인
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
            SoundManager.Instance?.PlaySfx(SfxType.Combo);
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
                SoundManager.Instance?.PlaySfx(SfxType.FoodEat);
                GameManager.Instance.AddScore(10);
                DataManager.Instance.AddFoodEaten(food.foodType); // 해금 시스템용 먹이 카운트
                //풀링 해야될듯
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
        moveInterval = Mathf.Max(minmoveInterval, moveInterval - moveSpeed); // 작을수록 빠름, min값 제한

    }

    /// <summary>상점에서 선택된 슬라임 헤드 스킨을 적용.</summary>
    private void ApplySelectedSkin()
    {
        if (skinEntries == null || skinEntries.Length == 0) return;

        string selectedId = DataManager.Instance.SelectedHeadId;
        foreach (var entry in skinEntries)
        {
            if (entry.itemId == selectedId && entry.sprite != null)
            {
                _sr.sprite = entry.sprite;
                return;
            }
        }
    }

    public void SetInputDirection(Vector2 _dir)
    {

        if(_dir != Vector2.zero && _dir != -_direction) //반대 방향 안돼
        {
            _inputDirection = _dir;

        }
    }
}
