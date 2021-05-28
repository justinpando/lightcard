using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityTaskManager;
using System.Linq;


[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour {

    public CanvasGroup canvasGroup;

    private Task sequence;
    
    public float fadeDuration = 0.5f;

    public bool visibleOnAwake = true;

    public bool setsInteractability = false;

    private float alpha = 0f;

    [Range(0f, 1f)]
    public float minAlpha = 0f;
    [Range(0f, 1f)]
    public float maxAlpha = 1f;
    
    public System.Action onFadeInComplete;
    public System.Action onFadeOutComplete;
    public List<Renderer> includedRenderers;
    private List<Tuple<Material, Color>> defaultrendererValues = new List<Tuple<Material, Color>>();  
    public bool isVisible = true;

    private bool debugging = false;
    
    void Awake()
    {
        if(canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        defaultrendererValues = includedRenderers.Select((r => new Tuple<Material, Color>(r.material, r.material.color))).ToList();
        SetVisibility(visibleOnAwake);
    }

    void setIncludedRenderers(float a) => defaultrendererValues.ForEach(r => {
            Color c = r.Item2;
            c.a *= a;
            r.Item1.color = c;
    });
    
    public void FadeIn()
    {
        if (isVisible) return;

        isVisible = true;
        
        if(debugging) Debug.Log($"Fade in: {gameObject.name}", this.gameObject);
        
        sequence?.Stop();
        sequence = new Task(FadeInSequence());
    }

    public void FadeOut()
    {
        if (!isVisible) return;
        
        isVisible = false;
        
        sequence?.Stop();
        sequence = new Task(FadeOutSequence());
    }

    public void SetVisibility(bool value) {
        float a = value ? maxAlpha : minAlpha;
        canvasGroup.alpha = a;
        setIncludedRenderers(a);
        
        if (setsInteractability)
        {
            canvasGroup.interactable = value;
            canvasGroup.blocksRaycasts = value;
        }
        
        isVisible = value;
    }

    public void FadeToVisibility(bool value)
    {
        if (value)
        {
            FadeIn();
        }
        else
        {
            FadeOut();
        }
    }
    
    IEnumerator FadeInSequence()
    {
        if(debugging) Debug.Log($"Fade in sequence starting: {gameObject.name}", this.gameObject);
        
        if (setsInteractability)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        isVisible = true;

        float timeElapsed = 0f;

        float startAlpha = canvasGroup.alpha;
        float distance = maxAlpha - startAlpha;
        
        float duration = fadeDuration * (distance);

        alpha = startAlpha;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            
            float i = Interpolate.Linear(startAlpha, distance, timeElapsed, duration);
            canvasGroup.alpha = i;
            setIncludedRenderers(i);
            
            yield return null;
        }

        onFadeInComplete?.Invoke();
    }

    IEnumerator FadeOutSequence()
    {
        if (setsInteractability)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        float startAlpha = canvasGroup.alpha;

        float timeElapsed = 0f;

        float distance = minAlpha - startAlpha;
        float duration = fadeDuration * Mathf.Abs(distance);
        
        while (timeElapsed < duration)
        {
            timeElapsed += Time.unscaledDeltaTime;

            float i = Interpolate.Linear(startAlpha, distance, timeElapsed, duration);
            canvasGroup.alpha = i;
            setIncludedRenderers(i);

            yield return null;
        }
        
        onFadeOutComplete?.Invoke();

        isVisible = false;
    }

    private void OnDestroy()
    {
        sequence?.Stop();
    }
}
