using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

        var newCollection = CreateInstance<CardCollection>();
        
        for (int n = 0; n < cells.Count; n++)
        {
            if(cells[n].value == "" || cells[n].value == "Name") continue;
            
            Card card = CreateInstance<Card>();
            card.name = cells[n].value;

            if(Enum.TryParse(ss[card.name, "Group"].value, out Card.Archetype archetype))
            {
                card.archetype = archetype;
                //Debug.Log($"Card {card.name}: Group found: {archetype}");
            }

            if(Enum.TryParse(ss[card.name, "Type"].value, out Card.Type type))
            {
                card.type = type;
            }

            if(int.TryParse(ss[card.name, "Cost"].value, out int cost))
            {
                card.cost = cost;
            }

            if(int.TryParse(ss[card.name, "Atk"].value, out int atk))
            {
                card.power = atk;
            }
            
            if(int.TryParse(ss[card.name, "Def"].value, out int def))
            {
                card.life = def;
            }

            try
            {
                card.description = ss[card.name, "Description"].value;
            }
            catch
            {
                card.description = "";
            }

            Debug.Log($"Created card {n}: {card.name}");
            importedCards.Add(card);
            
            AssetDatabase.CreateAsset(card, "Assets/Data/Cards/" + card.name + ".asset");
            
            newCollection.cards.Add(card);
        }

        string date = DateTime.Today.ToString("yyyy-MM-dd_HH-mm-ss");
        string collectionName = $"CardCollection_{date}";
        
        AssetDatabase.CreateAsset(newCollection, "Assets/Data/CardCollections/" + collectionName + ".asset");
        
        Debug.Log($"Imported {importedCards.Count} cards.");
    }
    
    public List<Card> importedCards;
}
