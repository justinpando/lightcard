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
        float a = value ? 1f : 0f;
        canvasGroup.alpha = a;
        setIncludedRenderers(a);
        
        if (setsInteractability)
        {
            canvasGroup.interactable = value;
            canvasGroup.blocksRaycasts = value;
        }
        
        isVisible = value;
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

        alpha = canvasGroup.alpha;
        float duration = fadeDuration * (1 - alpha);

        float originalAlpha = canvasGroup.alpha;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            alpha = originalAlpha + (timeElapsed / duration * (1 - originalAlpha));

            canvasGroup.alpha = alpha;

            setIncludedRenderers(alpha);
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

        float timeElapsed = 0f;

        alpha = canvasGroup.alpha;
        float duration = fadeDuration * alpha;

        float originalAlpha = canvasGroup.alpha;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.unscaledDeltaTime;

            alpha = originalAlpha - (timeElapsed / duration) ;

            canvasGroup.alpha = alpha;
            setIncludedRenderers(alpha);

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
