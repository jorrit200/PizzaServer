using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using PizzaServer.Responses;
using PizzaServer.Observers;

namespace PizzaServer.Servers;

public class TcpSubjectServer(int port) : IServerSubject, IHaveAes
{
    private readonly TcpListener _tcpListener = new(IPAddress.Parse("127.0.0.1"), port);
    private readonly RSACryptoServiceProvider _rsa = new();
    private Aes? _aes = null!;
    private readonly Dictionary<string, List<ISocketObserver>> _observers = new();

    public void Start()
    {
        Console.WriteLine(_rsa);
        _tcpListener.Start();
        Console.WriteLine("server started");
        var tcpClient = _tcpListener.AcceptTcpClient();
        Console.WriteLine("client connected");
        var stream = tcpClient.GetStream();

        while (true)
        {
            while (tcpClient.Available < 3)
            {
            }

            var bytes = new byte[tcpClient.Available];

            // ReSharper disable once MustUseReturnValue
            stream.Read(bytes, 0, bytes.Length);

            String data = EncodingHelper.ByteToStringUtf(bytes);

            if (!Regex.IsMatch(data, "^GET")) continue;
            var tcpResponse = new TcpResponse(stream);
            Console.WriteLine("We are getting a GET request");
            string requestType = new Regex("Type: (.*)").Match(data).Groups[1].Value.Trim();
            Notify(requestType, data, tcpResponse);
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
                    requireRsa.Update(message, response, _rsa, this);
                }
                else if (observer is ISocketObserverRequireAes requireAes)
                {
                    if (_aes == null)
                    {
                        throw new Exception("AES is required for this observer");
                    }
                    requireAes.Update(message, response, _aes);
                }
                else
                {
                    observer.Update(message, response);
                }
            }
        }
        else
        {
            Console.WriteLine("No observers for this request type");
        }
    }
    
    public void SetAes(Aes? aes)
    {
        _aes = aes;
    }
}