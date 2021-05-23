using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainContext : MonoBehaviour
{
    public MainViewController mainView;
    public DeckViewController deckView;
    public LibraryViewController libraryView;
    public OptionsViewController optionsView;
    
    public CardViewController cardViewPrefab;
    public List<CardData> cardCollection;
    
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        deckView.Initialize(cardViewPrefab);
        libraryView.Initialize(cardCollection, cardViewPrefab);
        
        mainView.gameObject.SetActive(true);
        deckView.gameObject.SetActive(false);
        libraryView.gameObject.SetActive(false);
        optionsView.gameObject.SetActive(false);
    }
}
