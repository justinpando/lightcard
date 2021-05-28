using UnityEngine;

public class MainContext : MonoBehaviour
{
    public MainViewController mainView;
    public DeckViewController deckView;
    public LibraryViewController libraryView;
    public OptionsViewController optionsView;
    
    public CardViewController cardViewPrefab;
    public FilterViewController filterViewPrefab;
    public FilterCollectionViewController filterCollectionPrefab;
    
    public CardLibrary library;
    
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        library.Initialize();
        
        deckView.Initialize(cardViewPrefab);

        FilterCollectionViewController filters = Instantiate(filterCollectionPrefab, libraryView.transform);
        filters.Initialize(library, filterViewPrefab);
        
        libraryView.Initialize(library, cardViewPrefab, filters);
        
        mainView.gameObject.SetActive(true);
        deckView.gameObject.SetActive(false);
        libraryView.gameObject.SetActive(false);
        optionsView.gameObject.SetActive(false);
    }
}
