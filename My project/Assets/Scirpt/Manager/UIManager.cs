using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    [Header("Gameplay UI")]
    [SerializeField] private ScoreUI scoreUI;
    [SerializeField] private PopupUI popupUI;

    [Header("Main UI")]
    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] private ShopUI shopUI;

    [Header("Modal")]
    [SerializeField] private Image modalBlocker;

    [Header("FadeEffect")]
    [SerializeField] private FadeUI fadeUI;


    public void Init()
    {
        InitGameplayUI();
        InitMainUI();
        SetupModalBlocker();
    }

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    private void InitGameplayUI()
    {
        if (scoreUI != null)
        {
            scoreUI.Init();
            scoreUI.Show();
        }

        if (popupUI != null)
        {
            popupUI.Init();
            popupUI.Hide();
        }
    }

    private void InitMainUI()
    {
        if (mainMenuUI == null)
        {
            return;
        }

        mainMenuUI.Init();
        mainMenuUI.SetCallbacks(OnClickStartGame, OpenShop, OnClickSetting, OnClickQuit);
        mainMenuUI.Show();

        if (shopUI != null)
        {
            shopUI.Init();
            shopUI.SetOnClose(CloseShop);
            shopUI.Hide();
        }
    }

    private void SetupModalBlocker()
    {
        if (modalBlocker != null)
        {
            modalBlocker.gameObject.SetActive(false);
            modalBlocker.raycastTarget = true;
            return;
        }

        Canvas rootCanvas = FindFirstObjectByType<Canvas>();
        if (rootCanvas == null)
        {
            return;
        }

        GameObject blockerObject = new GameObject("ModalBlocker", typeof(RectTransform), typeof(Image));
        blockerObject.transform.SetParent(rootCanvas.transform, false);

        RectTransform rect = blockerObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetSiblingIndex(rootCanvas.transform.childCount - 1);

        modalBlocker = blockerObject.GetComponent<Image>();
        modalBlocker.color = new Color(0f, 0f, 0f, 0.45f);
        modalBlocker.raycastTarget = true;
        modalBlocker.gameObject.SetActive(false);
    }

    public void UpdateScore(int _score)
    {
        if (scoreUI == null)
        {
            return;
        }

        scoreUI.SetScore(_score);
    }

    public void UpdateTime(float _time)
    {
        if (scoreUI == null)
        {
            return;
        }

        scoreUI.SetTime(_time);
    }

    public void OpenShop()
    {
        if (shopUI == null)
        {
            return;
        }

        SetModal(true);
        shopUI.Show();

        if (mainMenuUI != null)
        {
            mainMenuUI.Interactable(false);
        }
    }

    public void CloseShop()
    {
        if (shopUI != null)
        {
            shopUI.Hide();
        }

        if (mainMenuUI != null)
        {
            mainMenuUI.Interactable(true);
        }

        SetModal(false);
    }

    private void SetModal(bool value)
    {
        if (modalBlocker == null)
        {
            return;
        }

        modalBlocker.gameObject.SetActive(value);
    }

    private void OnClickStartGame()
    {
        if (mainMenuUI != null)
        {
            mainMenuUI.Hide();
        }

        Time.timeScale = 1f;
    }

    private void OnClickSetting()
    {
        ShowSimpleMessage("설정", "설정 UI는 아직 준비 중입니다.");
    }

    private void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowGameOverPopup(int _score, float _time, System.Action onRestart, System.Action onExit)
    {
        if (popupUI == null)
        {
            return;
        }

        string title = "게임 오버";
        string message = $"점수 : {_score}\n시간 : {_time:0.00}";

        popupUI.ShowChoice(
            title,
            message,
            "재시작",
            onRestart,
            "나가기",
            onExit,
            showCloseButton: false
        );
        popupUI.Show();
    }

    public void ShowSimpleMessage(string title, string message)
    {
        if (popupUI == null)
        {
            return;
        }

        popupUI.ShowMessage(title, message);
        popupUI.Show();
    }

    public void FadeEffect(float duration)
    {
        fadeUI.StartFadeOut(duration);
    }
}
