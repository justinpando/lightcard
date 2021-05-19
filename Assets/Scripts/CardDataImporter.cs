using System;
using System.Collections;
using System.Collections.Generic;
using GoogleSheetsToUnity;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

public class CardDataImporter : OdinEditorWindow
{
    [MenuItem("LightCard/Card Data Import")]
    private static void OpenWindow()
    {
        GetWindow<CardDataImporter>().Show();
    }

    public List<CardData> importedCards;
    
    [Button]
    public void UpdateFromSheet()
    {
        SpreadsheetManager.Read(new GSTU_Search("1yQOt8G8o4LON2B3nm3Oreq9GnfaWEn6Nx5Iourz_4Pw", "Cards", "A2", "Z500"), Callback);

    }

    private void Callback(GstuSpreadSheet ss)
    {
        List<GSTU_Cell> cells = ss.columns["Name"];

        for (int n = 0; n < cells.Count; n++)
        {
            if(cells[n].value == "") continue;
            
            CardData cardData = CreateInstance<CardData>();
            cardData.name = cells[n].value;

            if(Enum.TryParse(ss[cardData.name, "Group"].value, out CardData.Group group))
            {
                cardData.group = group;
            }

            if(Enum.TryParse(ss[cardData.name, "Type"].value, out CardData.Type type))
            {
                cardData.type = type;
            }

            cardData.cost = int.Parse(ss[cardData.name, "Cost"].value);
            cardData.attack = int.Parse(ss[cardData.name, "Atk"].value);
            cardData.defense = int.Parse(ss[cardData.name, "Def"].value);
            cardData.description = ss[cardData.name, "Description"].value;
            
            AssetDatabase.CreateAsset(cardData, "Assets/Data/Cards/" + cardData.name + ".asset");
            
            importedCards.Add(cardData);
        }
    }
}
