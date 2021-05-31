using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckItemView : MonoBehaviour
{
    public Image[] classImage;

    public Image bgImage;
    
    public TMP_Text nameText;
    public TMP_Text unitText;
    public TMP_Text abilityText;
    public TMP_Text charmText;

    public Button selectButton;
    public Button deleteButton;

    public DeckData deck;

    public System.Action OnSelectButtonPressed;
    public System.Action OnDeleteButtonPressed;
    private CardLibrary library;

    public void Initialize(CardLibrary library, DeckData deck)
    {
        this.library = library;
        this.deck = deck;

        deck.OnCardsUpdated += UpdateView;
        
        UpdateView();
        
        selectButton.onClick.AddListener(() => OnSelectButtonPressed?.Invoke());
        deleteButton.onClick.AddListener(() => OnDeleteButtonPressed?.Invoke());
    }

    private void UpdateView()
    {
        nameText.text = deck.name;

        if (deck.cards.Count == 0)
        {
            foreach (Image image in classImage)
            {
                image.gameObject.SetActive(false);
            }

            unitText.text = "0";
            abilityText.text = "0";
            charmText.text = "0";
            
            bgImage.color = Color.grey;
            
            return;
        }
        
        int unitCount = deck.cards.Count(x => x.type == CardData.Type.Unit);
        int abilityCount = deck.cards.Count(x => x.type == CardData.Type.Ability);
        int charmCount = deck.cards.Count(x => x.type == CardData.Type.Charm);

        unitText.text = unitCount.ToString();
        abilityText.text = abilityCount.ToString();
        charmText.text = charmCount.ToString();

        var cardsOrderedByGroup = deck.cards.OrderByDescending(x => x.@group);
        
        CardData.Group deckGroup = cardsOrderedByGroup.First().@group;

        var classData = library.classes.Find(x => x.group == deckGroup);
        
        foreach (Image image in classImage)
        {
            if(!image.gameObject.activeSelf) image.gameObject.SetActive(true);
            image.sprite = classData.symbol;
        }

        classImage[0].color = classData.secondaryColor;
        classImage[1].color = classData.primaryColor;

        bgImage.color = classData.secondaryColor;
    }
}
