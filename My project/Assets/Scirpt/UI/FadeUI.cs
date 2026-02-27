using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Rendering;
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
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 0.0f; //처음엔t 안보임

        isOut = true ;
    }



    public void StartFadeOut(float duration)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }


        if (isOut)
        {
            fadeRoutine = StartCoroutine(FadeIn(duration));
        }
        else 
        { 
            fadeRoutine = StartCoroutine(FadeOut(duration));
        }

    }


   

    private IEnumerator FadeOut(float duration)
    {
        canvasGroup.interactable = true;
        float startA = canvasGroup.alpha;

        float time = 0.0f;
        while (time < duration)
        { 
            time += Time.deltaTime;
            float k = Mathf.Clamp01(time / duration);
            canvasGroup.alpha = Mathf.Lerp(startA, 1f, k);
            yield return null;
        
        }
        isOut = true;
        canvasGroup.alpha = 1.0f;

        
    }

    private IEnumerator FadeIn(float duration)
    {
        float startA = canvasGroup.alpha;
        float time = 0.0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float k = Mathf.Clamp01(time / duration);
            canvasGroup.alpha = Mathf.Lerp(startA, 0.0f, k);
            yield return null;
        }


        canvasGroup.interactable = false;
        canvasGroup.alpha = 0.0f;
        isOut = false;


    }

}
