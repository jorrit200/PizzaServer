using System.Net.Sockets;
using System.Security.Cryptography;

namespace PizzaServer;
public interface ISocketObserver
{
    void Update(string requestType, string data, NetworkStream stream);
}

public interface ISocketObserverRequireRsa : ISocketObserver
{
    void Update(string requestType, string data, NetworkStream stream, RSACryptoServiceProvider rsa,
        TcpSubjectServer server);
}

public interface ISocketObserverRequireAes : ISocketObserver
{
    void Update(string requestType, string data, NetworkStream stream, Aes aes);
}

public interface ISocketSubject
{
    void Attach(ISocketObserver socketObserver, string requestType);
    void Detach(ISocketObserver socketObserver, string requestType);
    void Notify(string requestType, string message, NetworkStream stream);
}