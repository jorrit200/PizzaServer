// See https://aka.ms/new-console-template for more information

using System.Net;

namespace PizzaServer;

using System.Net.Sockets;

internal class PizzaServer
{
    public static void Main(string[] args)
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"),6789);
        tcpListener.Start();
        Console.WriteLine("server started");
        TcpClient tcpClient = tcpListener.AcceptTcpClient();
        Console.WriteLine("client connected");
        Console.WriteLine("Hello, World!");
    }
}