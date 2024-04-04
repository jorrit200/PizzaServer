using System.Text.RegularExpressions;   
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Sockets;

namespace PizzaServer;

public class TCPServer
{
    readonly int port;
    private TcpListener tcpListener;
    private RSACryptoServiceProvider rsa;
    public TCPServer(int port, string KeyPath)
    {
        this.port = port;
        this.tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
        
    }
}