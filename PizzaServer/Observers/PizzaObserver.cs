using System.Security.Cryptography;
using System.Text.RegularExpressions;
using PizzaServer.Responses;

namespace PizzaServer.Observers;

/// <summary>
/// Handles the ordering of a pizza
/// </summary>
public class PizzaObserver: ISocketObserverRequireAes
{
    private static Dictionary<string, int> _menu = new();
    private static Dictionary<string, int> _toppings = new();
    public PizzaObserver(Dictionary<string, int> menu, Dictionary<string, int> toppings)
    {
        _menu = menu;
        _toppings = toppings;
    }

    public void Update(string data, IResponse response, Aes aes)
    {
        string message = new Regex("Message: (.*)").Match(data).Groups[1].Value.Trim();
        string clientsIv = new Regex("IV: (.*)").Match(data).Groups[1].Value.Trim();
        string decodedMessage = EncodingHelper.DecodeMessage(message, aes, clientsIv);
        Console.WriteLine("Message: " + decodedMessage);

        if (decodedMessage.EndsWith("EOT;"))
        {
            decodedMessage = decodedMessage.Substring(0, decodedMessage.Length - 4);
        }
        var orderInterpreter = new OrderInterpreterMachine(decodedMessage.Split("\n").ToList().GetEnumerator(),
            _menu, _toppings);
        
        orderInterpreter.Interpret();
        
        Order order = orderInterpreter.ToOrder();

        byte[] responseMessage = EncodingHelper.BuildResponse($"Totale prijs: {order.GetTotalPrice()}.", aes);
        response.Send(responseMessage);
        
    }

    public void Update(string data, IResponse response)
    {
        throw new Exception("I require AES to work!");
    }
}