using UnityEngine;

public abstract class BaseUI : MonoBehaviour
{
    protected CanvasGroup canvasGroup;
    public bool isActive { get; private set; }

    protected bool _isInitialized = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Init()
    {
        if (_isInitialized)
        {
            return;
        }

        OnInitilze();
        _isInitialized = true;
    }

    public virtual void Hide()
    {
        if (!_isInitialized)
        {
            Init();
        }

        SetCanvsState(_alpha: 0f, _interactable: false, _blocksRaycasts: false);
        isActive = false;
        OnHide();
        gameObject.SetActive(false);
    }

    public void Interactable(bool value)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
    }

    public virtual void Show()
    {
        if (!_isInitialized)
        {
            Init();
        }

        gameObject.SetActive(true);
        SetCanvsState(_alpha: 1f, _interactable: true, _blocksRaycasts: true);
        isActive = true;
        OnShow();
    }

    protected void SetCanvsState(float _alpha, bool _interactable, bool _blocksRaycasts)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = _alpha;
        canvasGroup.interactable = _interactable;
        canvasGroup.blocksRaycasts = _blocksRaycasts;
    }

    protected virtual void OnInitilze() { }
    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
}
