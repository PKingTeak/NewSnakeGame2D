using System.Collections;
using UnityEngine;

/// <summary> 
/// 슬라임 헤드의 애니메이션을 전담하는 컴포넌트.
/// Animator가 있으면 트리거를 사용하고, 없으면 코드 기반 스케일 연출로 폴백.
/// SlimeController에서 이벤트 발생 시 메서드를 호출해 주세요.
/// </summary>
public class SlimeAnimator : MonoBehaviour
{
    // --- Animator 파라미터 이름 상수 ---
    private static readonly int TriggerEat    = Animator.StringToHash("Eat");
    private static readonly int TriggerCombo  = Animator.StringToHash("Combo");
    private static readonly int TriggerDeath  = Animator.StringToHash("Death");
    private static readonly int ParamMoveX    = Animator.StringToHash("MoveX");
    private static readonly int ParamMoveY    = Animator.StringToHash("MoveY");

    [Header("Code-based Animation (Animator 없을 때 사용)")]
    [SerializeField] private float eatScaleMultiplier = 1.3f;
    [SerializeField] private float eatDuration        = 0.15f;
    [SerializeField] private float squashAmount       = 0.15f; // 이동 시 찌그러짐 정도
    [SerializeField] private Color comboFlashColor    = Color.yellow;
    [SerializeField] private float comboFlashDuration = 0.3f;
    [SerializeField] private float deathShrinkDuration = 0.4f;

    private Animator     _animator;
    private SpriteRenderer _sr;
    private Vector3      _baseScale;
    private Color        _baseColor;

    private Coroutine _currentAnim;

    private void Awake()
    {
        _animator  = GetComponent<Animator>();
        _sr        = GetComponent<SpriteRenderer>();
        _baseScale = transform.localScale;
        _baseColor = _sr != null ? _sr.color : Color.white;
    }

    // ── 외부에서 호출하는 이벤트 메서드 ─────────────────────────────────

    /// <summary>이동 방향이 갱신될 때 호출. 방향에 따른 squash 연출.</summary>
    public void OnMove(Vector2 direction)
    {
        if (_animator != null)
        {
            _animator.SetFloat(ParamMoveX, direction.x);
            _animator.SetFloat(ParamMoveY, direction.y);
            return;
        }

        PlayCodeAnim(SquashAnim(direction));
    }

    /// <summary>음식을 먹었을 때 호출. 바운스 연출.</summary>
    public void OnEat()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(TriggerEat);
            return;
        }

        PlayCodeAnim(EatBounceAnim());
    }

    /// <summary>콤보가 발동됐을 때 호출. 플래시 연출.</summary>
    public void OnCombo()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(TriggerCombo);
            return;
        }

        PlayCodeAnim(ComboFlashAnim());
    }

    /// <summary>게임 오버 시 호출. 축소 후 비활성화 연출.</summary>
    public void OnDeath()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(TriggerDeath);
            return;
        }

        PlayCodeAnim(DeathShrinkAnim());
    }

    // ── 코드 기반 애니메이션 코루틴 ──────────────────────────────────────

    private void PlayCodeAnim(IEnumerator anim)
    {
        if (_currentAnim != null) StopCoroutine(_currentAnim);
        _currentAnim = StartCoroutine(anim);
    }

    // 이동 방향으로 살짝 찌그러지는 squash & stretch
    private IEnumerator SquashAnim(Vector2 dir)
    {
        float elapsed = 0f;
        float half = eatDuration * 0.5f;

        bool isHorizontal = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);
        Vector3 squashed = isHorizontal
            ? new Vector3(_baseScale.x * (1f + squashAmount), _baseScale.y * (1f - squashAmount), _baseScale.z)
            : new Vector3(_baseScale.x * (1f - squashAmount), _baseScale.y * (1f + squashAmount), _baseScale.z);

        while (elapsed < half)
        {
            transform.localScale = Vector3.Lerp(_baseScale, squashed, elapsed / half);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            transform.localScale = Vector3.Lerp(squashed, _baseScale, elapsed / half);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = _baseScale;
    }

    // 음식을 먹을 때 통통 튀는 bounce
    private IEnumerator EatBounceAnim()
    {
        Vector3 bigScale = _baseScale * eatScaleMultiplier;
        float half = eatDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            transform.localScale = Vector3.Lerp(_baseScale, bigScale, elapsed / half);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            transform.localScale = Vector3.Lerp(bigScale, _baseScale, elapsed / half);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = _baseScale;
    }

    // 콤보 시 색상 플래시
    private IEnumerator ComboFlashAnim()
    {
        if (_sr == null) yield break;

        float half = comboFlashDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            _sr.color = Color.Lerp(_baseColor, comboFlashColor, elapsed / half);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            _sr.color = Color.Lerp(comboFlashColor, _baseColor, elapsed / half);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _sr.color = _baseColor;
    }

    // 사망 시 축소 후 비활성화
    private IEnumerator DeathShrinkAnim()
    {
        float elapsed = 0f;

        while (elapsed < deathShrinkDuration)
        {
            transform.localScale = Vector3.Lerp(_baseScale, Vector3.zero, elapsed / deathShrinkDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
}
