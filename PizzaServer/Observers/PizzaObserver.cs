using System.Security.Cryptography;
using System.Text.RegularExpressions;
using PizzaServer.Responses;

namespace PizzaServer.Observers;

public class PizzaObserver: ISocketObserverRequireAes
{
    private static Dictionary<string, int> _menu = new();
    private static Dictionary<string, int> _toppings = new();
    public PizzaObserver(Dictionary<string, int> menu, Dictionary<string, int> toppings)
    {
        _menu = menu;
        _toppings = toppings;
    }

    public void Update(string requestType, string data, IResponse response, Aes aes)
    {
        string message = new Regex("Message: (.*)").Match(data).Groups[1].Value.Trim();
        string clientsIv = new Regex("IV: (.*)").Match(data).Groups[1].Value.Trim();
        string decodedMessage = EncodingHelper.DecodeMessage(message, aes, clientsIv);
        Console.WriteLine("Message: " + decodedMessage);

        

        byte[] responseMessage = EncodingHelper.BuildResponse("Budget approved, sending pizza", aes);
        response.Send(responseMessage);
        
    }

    public void Update(string requestType, string data, IResponse response)
    {
        throw new Exception("I require AES to work!");
    }
}