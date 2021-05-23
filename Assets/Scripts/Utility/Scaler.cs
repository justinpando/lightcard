using DG.Tweening;
using UnityEngine;

public class Scaler : MonoBehaviour
{
    public float duration;
    public float minScale = 1f;
    public float maxScale = 1f;
    
    public void ScaleUp()
    {
        transform.DOScale(maxScale, duration).SetEase(Ease.OutCubic);
    }

    public void ScaleDown()
    {
        transform.DOScale(minScale, duration).SetEase(Ease.OutCubic);
    }

    public void SetScale(float value)
    {
        transform.DOScale(value, duration).SetEase(Ease.OutCubic);
    }
}
