
public class CardFilter
{
    public string category;
    
    public delegate bool Filter(CardData card);
    public Filter isValid;

    public CardFilter(string category, Filter isValid)
    {
        this.category = category;
        this.isValid = isValid;
    }
}
