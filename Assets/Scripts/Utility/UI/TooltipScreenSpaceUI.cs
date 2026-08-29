using UnityEngine;
using TMPro;

public class TooltipScreenSpaceUI : MonoBehaviour
{

    [SerializeField]
    private RectTransform background;
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private RectTransform canvasRectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        Hide();
    }

    public void Show(string tooltipText)
    {
        gameObject.SetActive(true);
        SetText(tooltipText);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }


    private void Update()
    {
        Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;
        anchoredPosition = RestrictToScreen(anchoredPosition);        
        
        rectTransform.anchoredPosition = anchoredPosition;
    }

    private Vector2 RestrictToScreen(Vector2 anchoredPosition)
    {
        if (anchoredPosition.x + background.rect.width > canvasRectTransform.rect.width)
        {
            //Tooltip left screen on right side
            anchoredPosition.x = canvasRectTransform.rect.width - background.rect.width;
        }
        
        if (anchoredPosition.y + background.rect.height > canvasRectTransform.rect.height)
        {
            //Tooltip left screen on top side
            anchoredPosition.y = canvasRectTransform.rect.height - background.rect.height;
        }
        //Handle bottom and left sides
        Rect screenRect = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
        if (anchoredPosition.x < screenRect.x)
        {
            anchoredPosition.x = screenRect.x;
        }
        
        if(anchoredPosition.y < screenRect.y)
        {
            anchoredPosition.y = screenRect.y;
        }

        return anchoredPosition;
    }
    
    private void SetText(string toolTipText)
    {
        text.SetText(toolTipText);
        text.ForceMeshUpdate();

        Vector2 textSize = text.GetRenderedValues(false);
        Vector2 paddingSize = new Vector2(text.margin.x, text.margin.y) * 2;
        background.sizeDelta = textSize + paddingSize;
    }

}
