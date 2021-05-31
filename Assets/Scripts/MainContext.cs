using UnityEngine;
using UnityEngine.Serialization;

public class MainContext : MonoBehaviour
{
    public CardLibrary library;
    
    public MainViewController mainView;
    [FormerlySerializedAs("deckView")] public DeckCollectionViewController deckCollectionView;
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
        
        deckCollectionView.Initialize(library, cardViewPrefab, deckItemViewPrefab, deckEditorView);
        deckEditorView.Initialize(library, filters, cardViewPrefab, deckCardViewPrefab);
        
        libraryView.Initialize(library, cardViewPrefab, filters);
        
        mainView.gameObject.SetActive(true);
        deckCollectionView.gameObject.SetActive(false);
        libraryView.gameObject.SetActive(false);
        optionsView.gameObject.SetActive(false);
    }
}
