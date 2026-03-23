using System;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : BaseUI
{
    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    [Header("Volume Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private Action _onClose;

    public void SetOnClose(Action onClose) => _onClose = onClose;

    protected override void OnInitilze()
    {
        base.OnInitilze();
        closeButton?.onClick.AddListener(() => _onClose?.Invoke());

        bgmSlider?.onValueChanged.AddListener(v => SoundManager.Instance?.SetBgmVolume(v));
        sfxSlider?.onValueChanged.AddListener(v => SoundManager.Instance?.SetSfxVolume(v));

        Hide();
    }

    protected override void OnShow()
    {
        base.OnShow();
        // 열릴 때마다 저장된 볼륨 값으로 슬라이더 동기화
        if (bgmSlider != null) bgmSlider.value = DataManager.Instance?.BgmVolume ?? 1f;
        if (sfxSlider != null) sfxSlider.value = DataManager.Instance?.SfxVolume ?? 1f;
    }
}
