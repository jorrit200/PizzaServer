namespace PizzaServer;

public class Order
{
    private List<Pizza> _pizzas = new List<Pizza>();
    private List<int> _quantities = new List<int>();
    private string _customerName;
    private string _deliveryArea;
    private string _deliveryAddress;
    private string _deliveryTimeStamp;

    public Order(string customerName, string deliveryArea, string deliveryAddress, string deliveryTimeStamp)
    {
        _customerName = customerName;
        _deliveryArea = deliveryArea;
        _deliveryAddress = deliveryAddress;
        _deliveryTimeStamp = deliveryTimeStamp;
    }

    public void AddPizza(Pizza pizza, int quantity)
    {

        _pizzas.Add(pizza);
        _quantities.Add(quantity);
    }
    
    public int GetTotalPrice()
    {
        int total = 0;
        for (int i = 0; i < _pizzas.Count; i++)
        {
            total += _pizzas[i].GetPrice() * _quantities[i];
        }
        return total;
    }
}