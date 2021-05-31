using UnityEngine;

public class MainContext : MonoBehaviour
{
    public CardLibrary library;
    
    public MainViewController mainView;
    public DeckCollectionViewController deckView;
    public DeckEditorViewController deckEditorView;
    public LibraryViewController libraryView;
    public OptionsViewController optionsView;
    
    public CardViewController cardViewPrefab;
    public CardViewController deckCardViewPrefab;
    public FilterViewController filterViewPrefab;
    public FilterCollectionViewController filterCollectionPrefab;

    public DeckItemView deckItemViewPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        library.Initialize();
        
        FilterCollectionViewController filters = Instantiate(filterCollectionPrefab, libraryView.transform);
        filters.Initialize(library, filterViewPrefab);
        
        deckView.Initialize(library, cardViewPrefab, deckItemViewPrefab);
        deckEditorView.Initialize(library, filters, cardViewPrefab, deckCardViewPrefab);
        
        libraryView.Initialize(library, cardViewPrefab, filters);
        
        mainView.gameObject.SetActive(true);
        deckView.gameObject.SetActive(false);
        libraryView.gameObject.SetActive(false);
        optionsView.gameObject.SetActive(false);
    }
}
