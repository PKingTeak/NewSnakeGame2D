using TMPro;
using UnityEngine;

public class ScoreUI : BaseUI
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI scoreText;

    public void SetTime(float _time)
    {
        if (_time < 0)
        {
            _time = 0;
        }
        timeText.text = $"Time : {_time :0.00}";
    }

    public void SetScore(int _score)
    {
        scoreText.text = $"Score: {_score}";
    }

  
    
}
