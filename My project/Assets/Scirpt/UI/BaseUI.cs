using Unity.VisualScripting;
using UnityEditor.Analytics;
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
        _isInitialized = false;

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

    public void Show()
    {
        if (!_isInitialized)
        {
            Init();
        }
        this.gameObject.SetActive(true);

        
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

    protected virtual void OnInitilze() { } //초기화 해야 하는것들 켤때 말고 
    protected virtual void OnShow() { } //보여줄때 초기화 할것들
    protected virtual void OnHide() { } //소멸할때 해제해야 하는것들 
}
