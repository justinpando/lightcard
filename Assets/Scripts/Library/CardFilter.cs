
public class CardFilter
{
    public string category;
    
    public delegate bool Filter(Card card);
    public Filter isValid;

    public CardFilter(string category, Filter isValid)
    {
        this.category = category;
        this.isValid = isValid;
    }
}
