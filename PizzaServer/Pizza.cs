namespace PizzaServer;

public class Pizza
{
    private string Name;
    private int Price;
    private List<string> Toppings; 
    public Pizza(string name, Dictionary<string, int> fromMenu)
    {
        if (fromMenu.ContainsKey(name))
        {
            Name = name;
            Price = fromMenu[name];
            Toppings = new List<string>();
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
            Toppings.Add(topping);
            Price += value;
            return this;
        }
        throw new ArgumentException("Topping not found on menu");
    }
    
    public int GetPrice()
    {
        return Price;
    }
    
}