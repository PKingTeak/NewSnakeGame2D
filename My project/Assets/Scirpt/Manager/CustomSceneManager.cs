using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{ 
    GameScene,
    MainTitle,
}

public class CustomSceneManager : MonoSingleton<CustomSceneManager>
{
    
    private FadeUI fadeUI;
    private float duration  = 0.6f;

    protected override void Awake()
    {
        base.Awake();

        CreateFadeUI();
        SceneManager.sceneLoaded += OnSceneLoad;
               
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }



    private void CreateFadeUI()
    { 
        if (fadeUI != null)
        {
            return;
        }

        FadeUI fadeprefabs = Resources.Load<FadeUI>("UI/Fade");
        if (fadeprefabs == null)
        {
            Debug.LogError("Resources/UI/FadeUI.prefab 을 찾을 수 없습니다.");
            return;
        }

        fadeUI = Instantiate(fadeprefabs,transform,false);
        

    }

    public void ResetScene(SceneType type)
    {
        ChangeScene(type);
    }

    public void ChangeScene(SceneType type)
    {
        string sceneName = null;
        switch (type)
        {
            case SceneType.GameScene:
                sceneName = "GameScene";
                break;
            case SceneType.MainTitle:
                sceneName = "MainScene";
                break;
        }

        if (fadeUI == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        fadeUI.FadeOut(duration, () =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }


    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (fadeUI != null)
        {
            StartCoroutine(DelayDuration());
        }
    }

    private IEnumerator DelayDuration()
    {
        yield return null;
        fadeUI.FadeIn(duration);
    }


    private void FadeEffect()
    { 
        
    }


    

}
