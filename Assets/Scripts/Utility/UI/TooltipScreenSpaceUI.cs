using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class TooltipScreenSpaceUI : MonoBehaviour
{

    [SerializeField]
    private RectTransform background;
    [SerializeField]
    private TextMeshProUGUI text;
    
    private void Awake()
    {
        Debug.Log("Awake");
        
        SetText("Hello World!");
        
        TestTooltip();
    }

    private void SetText(string toolTipText)
    {
        text.SetText(toolTipText);
        text.ForceMeshUpdate();

        Vector2 textSize = text.GetRenderedValues(false);
        background.sizeDelta = textSize;
    }

    private async void TestTooltip()
    {
        SetText("Testing tooltip...");
        
        while (true)
        {
            await Task.Delay(5000);

            string abc = "sdfuiaudfpiadfhpuiIPAUSHPIDUHSH";
            string message = "I am a tooltip. I love my job.";

            for (int i = 0; i < Random.Range(5, 100); i++)
            {
                message += abc[Random.Range(0, abc.Length)];
            }
            
            SetText(message);
        }
    }
}
