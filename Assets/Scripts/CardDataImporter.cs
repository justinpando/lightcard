using System;
using System.Collections;
using System.Collections.Generic;
using GoogleSheetsToUnity;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class CardDataImporter : OdinEditorWindow
{
    [MenuItem("LightCard/Card Data Import")]
    private static void OpenWindow()
    {
        GetWindow<CardDataImporter>().Show();
    }

    [Button]
    public void UpdateFromSheet()
    {
        var search = new GSTU_Search("1yQOt8G8o4LON2B3nm3Oreq9GnfaWEn6Nx5Iourz_4Pw",
            "Cards", "A1", "G500", "C", 1);
        
        SpreadsheetManager.Read(search, Callback);
    }

    private void Callback(GstuSpreadSheet ss)
    {
        importedCards.Clear();
        
        List<GSTU_Cell> cells = ss.columns["Name"];

        Debug.Log($"Cell count: {cells.Count}");
        
        for (int n = 0; n < cells.Count; n++)
        {
            if(cells[n].value == "" || cells[n].value == "Name") continue;
            
            CardData cardData = CreateInstance<CardData>();
            cardData.name = cells[n].value;

            if(Enum.TryParse(ss[cardData.name, "Group"].value, out CardData.Group group))
            {
                cardData.group = group;
                Debug.Log($"Card {cardData.name}: Group found: {group}");
            }

            if(Enum.TryParse(ss[cardData.name, "Type"].value, out CardData.Type type))
            {
                cardData.type = type;
            }

            if(int.TryParse(ss[cardData.name, "Cost"].value, out int cost))
            {
                cardData.cost = cost;
            }

            if(int.TryParse(ss[cardData.name, "Atk"].value, out int atk))
            {
                cardData.attack = atk;
            }
            
            if(int.TryParse(ss[cardData.name, "Def"].value, out int def))
            {
                cardData.defense = def;
            }

            try
            {
                cardData.description = ss[cardData.name, "Description"].value;
            }
            catch
            {
                cardData.description = "";
            }

            Debug.Log($"Created card {n}: {cardData.name}");
            importedCards.Add(cardData);
            
            AssetDatabase.CreateAsset(cardData, "Assets/Data/Cards/" + cardData.name + ".asset");
        }
        
        Debug.Log($"Imported {importedCards.Count} cards.");
    }
    
    public List<CardData> importedCards;
}
