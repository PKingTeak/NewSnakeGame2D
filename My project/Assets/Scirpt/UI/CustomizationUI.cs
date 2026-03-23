using System;
using UnityEngine;
using UnityEngine.UI;
public class CustomizationUI : BaseUI
{
    [Header("Buttons")]
    [SerializeField] private Button closeButton;


    private Action _onClose;

    public void SetOnClose(Action onClose) => _onClose = onClose;
    protected override void OnInitilze()
    {
        base.OnInitilze();
        closeButton?.onClick.AddListener(() => _onClose?.Invoke());
        Hide();
    }
    
    //해금하지 못한 슬라임도 표시하면 좋을듯 함. 
}
