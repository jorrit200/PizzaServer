using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using PizzaServer.Responses;
using PizzaServer.Observers;

namespace PizzaServer.Servers;

public partial class TcpSubjectServer(int port) : IServerSubject, IHaveAes
{
    private readonly TcpListener _tcpListener = new(IPAddress.Parse("127.0.0.1"), port);
    private readonly RSACryptoServiceProvider _rsa = new();
    private Aes? _aes;
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

            var data = EncodingHelper.ByteToStringUtf(bytes);

            if (!VerbGetRegex().IsMatch(data)) continue;
            var tcpResponse = new TcpResponse(stream);
            Console.WriteLine("We are getting a GET request");
            var requestType = TypeHeaderRegex().Match(data).Groups[1].Value.Trim();
            Notify(requestType, data, tcpResponse);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    public void Attach(ISocketObserver socketObserver, string requestType)
    {
        if (!_observers.TryGetValue(requestType, out var requestTypeObserverList))
        {
            requestTypeObserverList = new List<ISocketObserver>();
            _observers[requestType] = requestTypeObserverList;
        }

        requestTypeObserverList.Add(socketObserver);
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
                switch (observer)
                {
                    case ISocketObserverRequireRsa requireRsa:
                        requireRsa.Update(message, response, _rsa, this);
                        break;
                    case ISocketObserverRequireAes when _aes == null:
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
    
    public void SetAes(Aes? aes)
    {
        _aes = aes;
    }

    [GeneratedRegex("Type: (.*)")]
    private static partial Regex TypeHeaderRegex();
    [GeneratedRegex("^GET")]
    private static partial Regex VerbGetRegex();
}