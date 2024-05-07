using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using PizzaServer.Responses;
using PizzaServer.Observers;

namespace PizzaServer.Servers;

public class UdpSubjectServer(int port): ISocketSubject, IHaveAes
{
    private readonly UdpClient _udpClient = new(port);
    private IPEndPoint _endPoint = new(IPAddress.Any, 0);
    
    private readonly RSACryptoServiceProvider _rsa = new();
    private Aes _aes = null!;

    private readonly Dictionary<string, List<ISocketObserver>> _observers = new();


    public void Start()
    {
        while (true)
        {
            byte[] receivedBytes = _udpClient.Receive(ref _endPoint);
            String receivedData = EncodingHelper.ByteToStringUtf(receivedBytes);
            if (Regex.IsMatch(receivedData, "^GET"))
            {
                string requestType = new Regex("Type: (.*)").Match(receivedData).Groups[1].Value.Trim();
                UdpResponse udpResponse = new UdpResponse(_udpClient, _endPoint);
                Notify(requestType, receivedData, udpResponse);
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }
    
    public void Attach(ISocketObserver socketObserver, string requestType)
    {
        if (!_observers.ContainsKey(requestType))
        {
            _observers[requestType] = new List<ISocketObserver>();
        }
        _observers[requestType].Add(socketObserver);
    }

    public void Detach(ISocketObserver socketObserver, string requestType)
    {
        if (_observers.TryGetValue(requestType, out var observer))
        {
            observer.Remove(socketObserver);
        }
    }

    public void Notify(string requestType, string message, IResponse response)
    {
        Console.WriteLine("Notifying observers: " + requestType);
        if (_observers.TryGetValue(requestType, out var requestedObservers))
        {
            foreach (var observer in requestedObservers)
            {
                if (observer is ISocketObserverRequireRsa requireRsa)
                {
                    if (_rsa == null)
                    {
                        throw new Exception("RSA is required for this observer");
                    }
                    requireRsa.Update(requestType, message, response, _rsa, this);
                }
                else if (observer is ISocketObserverRequireAes requireAes)
                {
                    if (_aes == null)
                    {
                        throw new Exception("AES is required for this observer");
                    }
                    requireAes.Update(requestType, message, response, _aes);
                }
                else
                {
                    observer.Update(requestType, message, response);
                }
            }
        }
        else
        {
            Console.WriteLine("No observers for this request type");
        }
    }

    public void SetAes(Aes aes)
    {
        _aes = aes;
    }
}