using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Gameplay UI")]
    [SerializeField] private ScoreUI scoreUI;
    [SerializeField] private PopupUI popupUI;

    [Header("Main UI")]
    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private CustomizationUI customizationUI;

    [Header("Modal")]
    [SerializeField] private Image modalBlocker;

    // ─────────────────────────────────────────────────────────────────────────
    // 팝업 시스템 - 데이터
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>JSON에서 로드한 팝업 설정 캐시. key = popupId</summary>
    private readonly Dictionary<string, PopupJsonData> _popupDataCache = new Dictionary<string, PopupJsonData>();

    /// <summary>actionId → 실제 Action 매핑 레지스트리</summary>
    private readonly Dictionary<string, Action> _actionRegistry = new Dictionary<string, Action>();

    // ─────────────────────────────────────────────────────────────────────────
    // Unity 생명주기
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[UIManager] 같은 씬에 중복 UIManager가 있습니다. 제거합니다: {name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Init();
    }

    public void Init()
    {
        InitGameplayUI();
        InitMainUI();
        SetupModalBlocker();
        LoadPopupData();          // 팝업 JSON 로드
        RegisterDefaultActions(); // 기본 액션 등록
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 초기화
    // ─────────────────────────────────────────────────────────────────────────

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
        if (mainMenuUI == null) return;

        mainMenuUI.Init();
        mainMenuUI.SetCallbacks(OnClickStartGame, OpenShop, OpenCustom, OnClickSetting, OnClickQuit);
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
        if (rootCanvas == null) return;

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

    // ─────────────────────────────────────────────────────────────────────────
    // 팝업 시스템 - JSON 로드
    // ─────────────────────────────────────────────────────────────────────────

    private void LoadPopupData()
    {
        var db = JsonLoader.LoadJson<PopupDatabase>("Data/popup_data");
        if (db?.popups == null)
        {
            Debug.LogWarning("[UIManager] 팝업 데이터 로드 실패.");
            return;
        }

        foreach (var popup in db.popups)
            _popupDataCache[popup.popupId] = popup;

        Debug.Log($"[UIManager] 팝업 {_popupDataCache.Count}개 로드 완료.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 팝업 시스템 - 액션 레지스트리
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>씬별로 필요한 액션을 등록합니다. (GameManager, ShopUI 등에서 호출)</summary>
    public void RegisterAction(string actionId, Action action)
    {
        _actionRegistry[actionId] = action;
    }

    /// <summary>기본으로 항상 사용되는 액션들을 등록합니다.</summary>
    private void RegisterDefaultActions()
    {
        RegisterAction("Close",       () => popupUI?.Hide());
        RegisterAction("QuitGame",    OnClickQuit);
        RegisterAction("GoToMainMenu",() => CustomSceneManager.Instance.ChangeScene(SceneType.MainTitle));
        RegisterAction("RestartGame", () => CustomSceneManager.Instance.ChangeScene(SceneType.GameScene));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 팝업 시스템 - 표시
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// JSON에 정의된 popupId로 팝업을 표시합니다.
    /// <para>contextActions: 이번 호출에서만 사용할 액션 오버라이드 (동적 데이터 전달용)</para>
    /// <para>messageOverride: JSON 메시지 대신 동적 문자열을 표시할 때 사용</para>
    /// </summary>
    public void ShowPopupById(string popupId,Dictionary<string, Action> contextActions = null,string messageOverride = null)
    {
        if (popupUI == null) return;

        if (!_popupDataCache.TryGetValue(popupId, out var config))
        {
            Debug.LogWarning($"[UIManager] '{popupId}' 팝업이 popup_data.json에 없습니다.");
            return;
        }

        var buttons = new List<PopupButtonData>();
        foreach (var btn in config.buttons)
        {
            // contextActions 우선 → _actionRegistry 순으로 탐색
            if (contextActions == null || !contextActions.TryGetValue(btn.actionId, out var action))
                _actionRegistry.TryGetValue(btn.actionId, out action);

            if (action == null)
                Debug.LogWarning($"[UIManager] actionId '{btn.actionId}'가 등록되지 않았습니다.");

            buttons.Add(new PopupButtonData(btn.label, action));
        }

        string message = messageOverride ?? config.message;
        popupUI.ShowPopup(config.title, message, buttons, config.showCloseButton);
        popupUI.Show();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 팝업 - 편의 메서드 (코드에서 직접 호출하는 경우)
    // ─────────────────────────────────────────────────────────────────────────

    public void ShowGameOverPopup(int score, float time, Action onRestart, Action onExit)
    {
        string message = $"점수 : {score}\n시간 : {time:0.00}";

        ShowPopupById("game_over",
            contextActions: new Dictionary<string, Action>
            {
                { "RestartGame", onRestart },
                { "GoToMainMenu", onExit }
            },
            messageOverride: message);
    }

    public void ShowSimpleMessage(string title, string message)
    {
        if (popupUI == null) return;

        popupUI.ShowMessage(title, message);
        popupUI.Show();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 상점
    // ─────────────────────────────────────────────────────────────────────────

    public void OpenShop()
    {
        if (shopUI == null) return;

        SetModal(true);
        shopUI.Show();

        if (mainMenuUI != null)
            mainMenuUI.Interactable(false);
    }

    public void OpenCustom() { }

    public void CloseShop()
    {
        if (shopUI != null)
            shopUI.Hide();

        if (mainMenuUI != null)
            mainMenuUI.Interactable(true);

        SetModal(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 씬 이벤트
    // ─────────────────────────────────────────────────────────────────────────

    private void OnClickStartGame()
    {
        if (mainMenuUI != null)
            mainMenuUI.Interactable(false);

        Time.timeScale = 1f;
        CustomSceneManager.Instance.ChangeScene(SceneType.GameScene);
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

    // ─────────────────────────────────────────────────────────────────────────
    // UI 업데이트
    // ─────────────────────────────────────────────────────────────────────────

    public void UpdateScore(int score)
    {
        if (scoreUI == null) return;
        scoreUI.SetScore(score);
    }

    public void UpdateTime(float time)
    {
        if (scoreUI == null) return;
        scoreUI.SetTime(time);
    }

    private void SetModal(bool value)
    {
        if (modalBlocker == null) return;
        modalBlocker.gameObject.SetActive(value);
    }
}
