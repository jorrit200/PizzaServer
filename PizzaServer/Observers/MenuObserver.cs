using System.Security.Cryptography;
using PizzaServer.Responses;
using PizzaServer;
using PizzaServer.Observers;

namespace PizzaServer.Observers;

/// <summary>
/// Sends a menu with available pizzas
/// </summary>
public class MenuObserver: ISocketObserverRequireAes
{
    private static Dictionary<string, int> _menu = new();
    
    public MenuObserver(Dictionary<string, int> menu)
    {
        _menu = menu;
    }
    
    public void Update(string data, IResponse response, Aes? aes)
    {
        response.Send(
    EncodingHelper.BuildResponse(
                DictionaryToString(_menu), aes
            )
        );
    }
    
    public void Update(string data, IResponse response)
    {
        throw new Exception("I require aes to work");
    }

    static string DictionaryToString(Dictionary<string, int> dictionary) =>
        dictionary.Aggregate("", (current, item) =>
            $"{current}{(item.Key + ": " + item.Value + PizzaServer.Eol)}");
}