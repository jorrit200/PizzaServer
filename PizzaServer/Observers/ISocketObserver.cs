using System.Security.Cryptography;
using PizzaServer.Responses;

namespace PizzaServer.Observers;
public interface ISocketObserver
{
    void Update(string requestType, string data, IResponse response);
}

public interface ISocketObserverRequireRsa : ISocketObserver
{
    void Update(string requestType, string data, IResponse response, RSACryptoServiceProvider rsa,
        IHaveAes server);
}

public interface ISocketObserverRequireAes : ISocketObserver
{
    void Update(string requestType, string data, IResponse response, Aes aes);
}

public interface ISocketSubject
{
    void Attach(ISocketObserver socketObserver, string requestType);
    void Detach(ISocketObserver socketObserver, string requestType);
    void Notify(string requestType, string message, IResponse response);
}