using UnityEngine;

public class UIManager :MonoSingleton<UIManager>
{
    [SerializeField]
    private ScoreUI scoreUI;
   

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
    

}