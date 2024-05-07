using System.Net;
using System.Net.Sockets;

namespace PizzaServer;

public class UdpResponse(UdpClient udpClient, IPEndPoint responseIp) : IResponse
{
    public void Send(byte[] message)
    {
        udpClient.Send(message, message.Length, responseIp);
    }
}