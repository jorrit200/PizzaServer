using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using PizzaServer.Responses;
using PizzaServer.Observers;

namespace PizzaServer.Servers;

public partial class UdpSubjectServer(int port): IServerSubject, IHaveAes
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
            var receivedBytes = _udpClient.Receive(ref _endPoint);
            var receivedData = EncodingHelper.ByteToStringUtf(receivedBytes);
            if (!Regex.IsMatch(receivedData, "^GET")) continue;
            var requestType = MyRegex().Match(receivedData).Groups[1].Value.Trim();
            var udpResponse = new UdpResponse(_udpClient, _endPoint);
            Notify(requestType, receivedData, udpResponse);
        }
        // ReSharper disable once FunctionNeverReturns
    }
    
    public void Attach(ISocketObserver socketObserver, string requestType)
    {
        if (!_observers.ContainsKey(requestType))
        {
            _observers[requestType] = [];
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
        Console.WriteLine($"Notifying observers: {requestType}");
        if (_observers.TryGetValue(requestType, out var requestedObservers))
        {
            foreach (var observer in requestedObservers)
            {
                switch (observer)
                {
                    case ISocketObserverRequireRsa requireRsa when _rsa == null:
                        throw new Exception("RSA is required for this observer");
                    case ISocketObserverRequireRsa requireRsa:
                        requireRsa.Update(message, response, _rsa, this);
                        break;
                    case ISocketObserverRequireAes requireAes when _aes == null:
                        throw new Exception("AES is required for this observer");
                    case ISocketObserverRequireAes requireAes:
                        requireAes.Update(message, response, _aes);
                        break;
                    default:
                        observer.Update(message, response);
                        break;
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

    [GeneratedRegex("Type: (.*)")]
    private static partial Regex MyRegex();
}