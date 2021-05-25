using UnityEngine;

public class MainContext : MonoBehaviour
{
    public MainViewController mainView;
    public DeckViewController deckView;
    public LibraryViewController libraryView;
    public OptionsViewController optionsView;
    
    public CardViewController cardViewPrefab;
    public FilterViewController filterViewPrefab;
    
    public CardLibrary library;
    
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        deckView.Initialize(cardViewPrefab);
        libraryView.Initialize(library, cardViewPrefab, filterViewPrefab);
        
        mainView.gameObject.SetActive(true);
        deckView.gameObject.SetActive(false);
        libraryView.gameObject.SetActive(false);
        optionsView.gameObject.SetActive(false);
    }
}
