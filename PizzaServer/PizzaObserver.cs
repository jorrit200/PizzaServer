using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace PizzaServer;

public class PizzaObserver: ISocketObserverRequireAes
{
    private static Dictionary<string, int> _menu = new();
    private static Dictionary<string, int> _toppings = new();
    public PizzaObserver(Dictionary<string, int> menu, Dictionary<string, int> toppings)
    {
        _menu = menu;
        _toppings = toppings;
    }
    
    public void Update(string requestType, string data, NetworkStream stream, Aes aes)
    {
        string message = new Regex("Message: (.*)").Match(data).Groups[1].Value.Trim();
        string clientsIv = new Regex("IV: (.*)").Match(data).Groups[1].Value.Trim();
        string decodedMessage = EncodingHelper.DecodeMessage(message, aes, clientsIv);
        Console.WriteLine("Message: " + decodedMessage);

        List<string> orderParts = decodedMessage.Split('\n').ToList();
        string address = orderParts[0];
        string postalCode = orderParts[1];
        List<string> pizzaLines = orderParts.GetRange(2, orderParts.Count - 3);
        string time = orderParts[^1];

        Order pizzas = new Order();
        
        bool newPizzaLine = true;
        bool pizzaQuantityLine = false;
        int toppics = 0;
        bool toppicsLine = false;
        int quantity = 0;
        Pizza pizza = new Pizza("Margherita", _menu);
        foreach (var pizzaline in pizzaLines)
        {
            if (newPizzaLine)
            {
                pizza = new Pizza(pizzaline, _menu);
                // pizzas.AddPizza(pizza);
                newPizzaLine = false;
                pizzaQuantityLine = true;
            }
            else if (pizzaQuantityLine)
            {
                quantity = int.Parse(pizzaline);
                pizzaQuantityLine = false;
            }
            else if (toppicsLine)
            {
                toppics = int.Parse(pizzaline);
                toppicsLine = false;
            }
            else if (toppics > 0)
            {
                pizza.WithTopping(pizzaline, _toppings);
                toppics--;
                if (toppics == 0)
                {
                    pizzas.AddPizza(pizza, quantity);
                    newPizzaLine = true;
                }
            }
        }

        byte[] response = EncodingHelper.BuildResponse("Price: " + pizzas.GetTotalPrice(), aes);
        stream.Write(response, 0, response.Length);
    }
    
    public void Update(string requestType, string data, NetworkStream stream)
    {
        throw new Exception("I require AES to work!");
    }
}