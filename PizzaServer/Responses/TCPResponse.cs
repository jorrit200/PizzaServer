using System.Net.Sockets;

namespace PizzaServer;

public class TcpResponse(NetworkStream stream) : IResponse
{
    public void Send(byte[] message)
    {
        stream.Write(message, 0, message.Length);
    }
}