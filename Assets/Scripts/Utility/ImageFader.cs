using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class ImageFader : MonoBehaviour {

    public Image image;

    private IEnumerator sequence;
    
    public float fadeDuration = 0.5f;

    public bool visibleOnAwake = true;

    private float alpha = 0f;

    public Color desiredColor;
    
    public System.Action onFadeInComplete;
    public System.Action onFadeOutComplete;
    
    void Awake()
    {
        if(image == null) image = GetComponent<Image>();

        desiredColor = image.color;
        
        SetVisibility(visibleOnAwake);
    }

    public void FadeIn()
    {
        ResetSequence();
        sequence = FadeInSequence();
        StartCoroutine(sequence);
    }

    public void FadeOut()
    {
        ResetSequence();
        sequence = FadeOutSequence();
        StartCoroutine(sequence);
    }
    
    void ResetSequence()
    {
        if (sequence != null)
        {
            StopCoroutine(sequence);
        }
    }

    public void SetVisibility(bool value)
    {
        Color newColor = default;
        newColor.a = value ? 1f : 0f;
        image.color = newColor;
    }

    IEnumerator FadeInSequence()
    {
        image.DOKill();
        
        float timeElapsed = 0f;
        
        float duration = fadeDuration * (1 - image.color.a);

        DOTween.ToAlpha(
            () => image.color,
            color => image.color = color,
            1f, // target alpha.
            duration // time.
        );
        
        while (timeElapsed < duration)
        {
            timeElapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        onFadeInComplete?.Invoke();
    }

    IEnumerator FadeOutSequence()
    {
        image.DOKill();
        
        float timeElapsed = 0f;
        
        float duration = fadeDuration * image.color.a;
        
        DOTween.ToAlpha(
            () => desiredColor,
            color => image.color = color,
            0f, // target alpha.
            duration // time.
        );
        
        while (timeElapsed < duration)
        {
            timeElapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        onFadeOutComplete?.Invoke();
    }

    private void OnDestroy()
    {
        ResetSequence();
    }
}
