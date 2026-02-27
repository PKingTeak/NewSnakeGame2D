using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : BaseUI
{
    [Header("Main Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button quitButton;

    private Action _onStart;
    private Action _onShop;
    private Action _onSetting;
    private Action _onQuit;


   

    protected override void OnInitilze()
    {
        BindButton(startButton, () => _onStart?.Invoke());
        BindButton(shopButton, () => _onShop?.Invoke());
        BindButton(settingButton, () => _onSetting?.Invoke());
        BindButton(quitButton, () => _onQuit?.Invoke());

        base.OnInitilze();
    }

    public void SetCallbacks(Action onStart, Action onShop, Action onSetting, Action onQuit)
    {
        _onStart = onStart;
        _onShop = onShop;
        _onSetting = onSetting;
        _onQuit = onQuit;
    }

    private void BindButton(Button targetButton, Action action)
    {
        if (targetButton == null)
        {
            return;
        }

        targetButton.onClick.RemoveAllListeners();
        targetButton.onClick.AddListener(() => action?.Invoke());
    }

    
}
