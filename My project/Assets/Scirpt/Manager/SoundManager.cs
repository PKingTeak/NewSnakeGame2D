using UnityEngine;

public enum BgmType { MainMenu, Game }

public enum SfxType
{
    FoodEat,
    Combo,
    GameOver,
    ButtonClick,
    Purchase,
    Unlock,
}

public class SoundManager : MonoSingleton<SoundManager>
{
    [Header("AudioSource (비워두면 자동 생성)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM 클립")]
    [SerializeField] private AudioClip mainMenuBgm;
    [SerializeField] private AudioClip gameBgm;

    [Header("SFX 클립")]
    [SerializeField] private AudioClip foodEatSfx;
    [SerializeField] private AudioClip comboSfx;
    [SerializeField] private AudioClip gameOverSfx;
    [SerializeField] private AudioClip buttonClickSfx;
    [SerializeField] private AudioClip purchaseSfx;
    [SerializeField] private AudioClip unlockSfx;

    protected override void Awake()
    {
        base.Awake();

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop        = true;
            bgmSource.playOnAwake = false;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop        = false;
            sfxSource.playOnAwake = false;
        }

        ApplySavedVolume();
    }

    // ─── BGM ─────────────────────────────────────────────────────────────────

    public void PlayBgm(BgmType type)
    {
        AudioClip clip = type == BgmType.MainMenu ? mainMenuBgm : gameBgm;
        if (clip == null || bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBgm() => bgmSource.Stop();

    // ─── SFX ─────────────────────────────────────────────────────────────────

    public void PlaySfx(SfxType type)
    {
        AudioClip clip = GetSfxClip(type);
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    private AudioClip GetSfxClip(SfxType type) => type switch
    {
        SfxType.FoodEat     => foodEatSfx,
        SfxType.Combo       => comboSfx,
        SfxType.GameOver    => gameOverSfx,
        SfxType.ButtonClick => buttonClickSfx,
        SfxType.Purchase    => purchaseSfx,
        SfxType.Unlock      => unlockSfx,
        _                   => null
    };

    // ─── 볼륨 ────────────────────────────────────────────────────────────────

    public float BgmVolume => bgmSource.volume;
    public float SfxVolume => sfxSource.volume;

    public void SetBgmVolume(float value)
    {
        bgmSource.volume = value;
        DataManager.Instance?.SetBgmVolume(value);
    }

    public void SetSfxVolume(float value)
    {
        sfxSource.volume = value;
        DataManager.Instance?.SetSfxVolume(value);
    }

    private void ApplySavedVolume()
    {
        if (DataManager.Instance == null) return;
        bgmSource.volume = DataManager.Instance.BgmVolume;
        sfxSource.volume = DataManager.Instance.SfxVolume;
    }
}
