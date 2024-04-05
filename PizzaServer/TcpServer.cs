using System.Text.RegularExpressions;   
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Sockets;

namespace PizzaServer;

public class TcpServer
{
    readonly int port;
    private TcpListener _tcpListener;
    private RSACryptoServiceProvider _rsa;
    private RSACryptoServiceProvider _clientRsa;
    private Aes _aes;
    private bool _clientConnected = false;
    private bool _clientKeyRecieved = false;
    public TcpServer(int port, string KeyPath)
    {
        this.port = port;
        _tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
        
    }
}