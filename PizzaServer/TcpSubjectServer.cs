using System.Text.RegularExpressions;   
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Sockets;

namespace PizzaServer;

public class TcpSubjectServer(int port) : ISocketSubject
{
    readonly int port = port;
    private TcpListener _tcpListener = new(IPAddress.Parse("127.0.0.1"), port);
    private RSACryptoServiceProvider _rsa = new();
    private RSACryptoServiceProvider _clientRsa;
    private Aes _aes;
    private bool _clientConnected = false;
    private bool _clientKeyRecieved = false;
    private Dictionary<string, List<ISocketObserver>> observers = new();

    public void Start()
    {
        Console.WriteLine(_rsa);
        _tcpListener.Start();
        Console.WriteLine("server started");
        TcpClient tcpClient = _tcpListener.AcceptTcpClient();
        Console.WriteLine("client connected");
        NetworkStream stream = tcpClient.GetStream();

        while (true)
        {
            while (tcpClient.Available < 3)
            {
            }

            byte[] bytes = new byte[tcpClient.Available];

            stream.Read(bytes, 0, bytes.Length);

            String data = EncoderHelper.ByteToStringUtf(bytes);

            if (Regex.IsMatch(data, "^GET"))
            {
                Console.WriteLine("We are getting a GET request");
                string requestType = new Regex("Type: (.*)").Match(data).Groups[1].Value.Trim();
                Notify(requestType, data, stream);
            }
        }
    }

    public void Attach(ISocketObserver socketObserver, string requestType)
    {
        if (!observers.ContainsKey(requestType))
        {
            observers[requestType] = new List<ISocketObserver>();
        }
        observers[requestType].Add(socketObserver);
    }

    public void Detach(ISocketObserver socketObserver, string requestType)
    {
        if (observers.ContainsKey(requestType))
        {
            observers[requestType].Remove(socketObserver);
        }
    }

    public void Notify(string requestType, string message, NetworkStream stream)
    {
        if (observers.TryGetValue(requestType, out var RequestedObservers))
        {
            foreach (var observer in RequestedObservers)
            {
                if (observer is ISocketObserverRequireRsa requireRsa)
                {
                    if (_rsa == null)
                    {
                        throw new Exception("RSA is required for this observer");
                    }
                    requireRsa.Update(requestType, message, stream, _rsa);
                }
                else if (observer is ISocketObserverRequireAes requireAes)
                {
                    if (_aes == null)
                    {
                        throw new Exception("AES is required for this observer");
                    }
                    requireAes.Update(requestType, message, stream, _aes);
                }
                else
                {
                    observer.Update(requestType, message, stream);
                }
            }
        }
        else
        {
            Console.WriteLine("No observers for this request type");
        }
    }
}