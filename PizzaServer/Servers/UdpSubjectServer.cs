using System.Net;
using System.Net.Sockets;

namespace PizzaServer.Servers;

public class UdpSubjectServer(int port): ISocketSubject
{
    private readonly int _port = port;
    private UdpClient _udpClient = new UdpClient(port);
    private IPEndPoint _endPoint = new IPEndPoint(IPAddress.Any, 0);


    public void Start()
    {
        while (true)
        {
            byte[] receivedBytes = _udpClient.Receive(ref _endPoint);
            string receivedData = EncodingHelper.ByteToStringUtf(receivedBytes);
        }
    }
    
    public void Attach(ISocketObserver socketObserver, string requestType)
    {
        throw new NotImplementedException();
    }

    public void Detach(ISocketObserver socketObserver, string requestType)
    {
        throw new NotImplementedException();
    }

    public void Notify(string requestType, string message, IResponse response)
    {
        throw new NotImplementedException();
    }
}