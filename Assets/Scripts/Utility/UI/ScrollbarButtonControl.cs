using UnityEngine;
using UnityEngine.UI;

public class ScrollbarButtonControl : MonoBehaviour
{
    public Scrollbar target;
    public Button incrementButton;
    public Button decrementButton;
    public float step = 0.08f;

    public bool resetValueOnStart = true;

    void Start()
    {
        incrementButton.onClick.AddListener(Increment);
        decrementButton.onClick.AddListener(Decrement);
        if (resetValueOnStart)
        {
            target.value = 1f;
            incrementButton.interactable = false;
        }
    }
    
    public void Increment()
    {
        target.value = Mathf.Clamp(target.value + step, 0, 1);
        incrementButton.interactable = target.value < 1;
        decrementButton.interactable = true;
    }
 
    public void Decrement()
    {
        target.value = Mathf.Clamp(target.value - step, 0, 1);
        decrementButton.interactable = target.value > 0;;
        incrementButton.interactable = true;
    }
}