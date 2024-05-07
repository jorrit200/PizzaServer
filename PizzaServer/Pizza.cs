namespace PizzaServer;

public class Pizza
{
    private string _name;
    private int _price;
    private readonly List<string> _toppings; 
    public Pizza(string name, Dictionary<string, int> fromMenu)
    {
        if (fromMenu.TryGetValue(name, out var value))
        {
            _name = name;
            _price = value;
            _toppings = new List<string>();
        }
        else
        {
            throw new ArgumentException("Pizza not found on menu");
        
        }
    }
    
    public Pizza WithTopping(string topping, Dictionary<string, int> fromMenu)
    {
        if (fromMenu.TryGetValue(topping, out var value))
        {
            _toppings.Add(topping);
            _price += value;
            return this;
        }
        throw new ArgumentException("Topping not found on menu");
    }
    
    public int GetPrice()
    {
        return _price;
    }
    
}