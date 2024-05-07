using System.Net.Sockets;
using System.Security.Cryptography;

namespace PizzaServer;

public class MenuObserver: ISocketObserverRequireAes
{
    private static Dictionary<string, int> _menu = new();
    
    public MenuObserver(Dictionary<string, int> menu)
    {
        _menu = menu;
    }
    
    public void Update(string requestType, string data, IResponse response, Aes aes)
    {
        throw new NotImplementedException();
    }
    
    public void Update(string requestType, string data, IResponse response)
    {
        throw new Exception("I need AES to work!");
    }

    static string DictionaryToString(Dictionary<string, int> dictionary)
    {
        string result = "";
        foreach (var item in dictionary)
        {
            result += item.Key + ": " + item.Value + PizzaServer.Eol;
        }
        return result;
    }

}