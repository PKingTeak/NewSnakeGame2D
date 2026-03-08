using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using System.Diagnostics.Contracts;
public class FadeUI : MonoBehaviour
{

    
    private CanvasGroup canvasGroup;

    private Coroutine fadeRoutine;

    private bool isOut;

    private void Awake()
    {
       

        if (canvasGroup == null)
        {
        canvasGroup = GetComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;                
        canvasGroup.interactable = false;      
        canvasGroup.blocksRaycasts = false;    
        gameObject.SetActive(false);           

        isOut = true ;
    }

    public void FadeOut(float duration, Action onComplete = null)
    {
        StartFade(1f, duration,true, onComplete);
    }

    public void FadeIn(float duration, Action onComplete = null)
    {
        Debug.Log($"[FadeIn 호출] 현재 alpha = {canvasGroup.alpha}");
        StartFade(0f, duration, false, onComplete); //반대
    }


    private void StartFade(float targetAlpha, float duration, bool blockInput, Action onComplete)
    {
        if (fadeRoutine != null)
        { 
            StopCoroutine(fadeRoutine);
        }

        gameObject.SetActive(true);
        canvasGroup.interactable = blockInput;
        canvasGroup.blocksRaycasts = blockInput;

        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration, onComplete));
    }



    private IEnumerator FadeRoutine(float targetAlpha, float duration, Action complete)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime; //이걸로 해야 팝업이나 정지 상태에서도 fade가 실행된다. 독립적인 시간을 따로 사용해야함 
            float t = Mathf.Clamp01(time/duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;

        }
        canvasGroup.alpha = targetAlpha;
        if (Mathf.Approximately(targetAlpha, 0f))
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        complete?.Invoke();
    }

   

  

}
