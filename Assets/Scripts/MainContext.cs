using UnityEngine;
using UnityEngine.Serialization;

public class MainContext : MonoBehaviour
{
    public CardLibrary library;
    
    public MainViewController mainView;
    
    public DeckCollectionViewController deckCollectionView;
    public DeckEditorViewController deckEditorView;
    public DeckItemView deckItemViewPrefab;
    
    public LibraryViewController libraryView;
    public OptionsViewController optionsView;
    
    public CardViewController cardViewPrefab;
    public CardViewController deckCardViewPrefab;
    public FilterViewController filterViewPrefab;
    public FilterCollectionViewController filterCollectionPrefab;

    public SaveDataManager saveManager;
    
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        saveManager = new SaveDataManager(library);
        SaveData saveData = saveManager.Load();
        
        library.Initialize(saveData);
        
        FilterCollectionViewController filters = Instantiate(filterCollectionPrefab, libraryView.transform);
        filters.Initialize(library, filterViewPrefab);
        
        deckCollectionView.Initialize(library, cardViewPrefab, deckItemViewPrefab, deckEditorView, saveManager);
        deckEditorView.Initialize(library, filters, cardViewPrefab, deckCardViewPrefab, saveManager);
        
        libraryView.Initialize(library, cardViewPrefab, filters);
        
        mainView.gameObject.SetActive(true);
        deckCollectionView.gameObject.SetActive(false);
        libraryView.gameObject.SetActive(false);
        optionsView.gameObject.SetActive(false);
        
        
        //Verb classes
        //Triggers, conditionals
        //pass the verb class to the card, the card passes the verb class to each of the affixes
        //I'm receiving stab damage, pass it to effects like poisoned, armor, keyword etc, the keyword acts on the received action
        //If this happens, do this unless. 2 step process. Should I .. deal damage etc
    }
}
