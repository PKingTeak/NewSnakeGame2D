using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUpButtonUI : MonoBehaviour
{
    [SerializeField] private Button btn;
    [SerializeField] private TMP_Text label_text;

    public void SetButton(string _label, Action action)
    { 
        label_text.text = _label;

        btn.onClick.RemoveAllListeners();

        btn.onClick.AddListener(() =>
        {
            action?.Invoke();
        });
        
    }


    public void Clear()
    {
        btn.onClick.RemoveAllListeners();
        label_text.text = string.Empty;
    }

}
