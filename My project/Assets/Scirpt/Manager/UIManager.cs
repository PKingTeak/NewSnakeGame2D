using UnityEngine;

public class UIManager :MonoSingleton<UIManager>
{
    [SerializeField]
    private ScoreUI scoreUI;
    [SerializeField]
    private PopupUI popupUI;

    public void Init()
    {
        scoreUI.Init();
        scoreUI.Show();
    }

    public void UpdateScore(int _score)
    {
        scoreUI.SetScore(_score);
    }

    public void UpdateTime(float _time)
    {
        scoreUI.SetTime(_time);
    }

    public void ShowGameOverPopup(int _score, float _time, System.Action onRestart, System.Action onExit)
    {
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
    }

    public void ShowSimpleMessage(string title, string message)
    {
        popupUI.ShowMessage(title, message);
    }
    

}