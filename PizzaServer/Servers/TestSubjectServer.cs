using System.Security.Cryptography;
using PizzaServer.Observers;
using PizzaServer.Responses;

namespace PizzaServer.Servers;

public class TestSubjectServer: IServerSubject, IHaveAes 
{
    private readonly Dictionary<string, List<ISocketObserver>> _observers = new();

    private readonly RSACryptoServiceProvider _rsa = new();
    private Aes? _aes;
    
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
        if (!_observers.TryGetValue(requestType, out var requestedObservers)) return;
        foreach (var requestedObserver in requestedObservers)
        {
            switch (requestedObserver)
            {
                case ISocketObserverRequireRsa requireRsa:
                    requireRsa.Update(message, response, _rsa, this);
                    break;
                case ISocketObserverRequireAes when _aes == null:
                    throw new Exception(
                        "This server does not yet have an AES key, so it can not handle this request");
                case ISocketObserverRequireAes requireAes:
                    requireAes.Update(message, response, _aes);
                    break;
                default:
                    requestedObserver.Update(message, response);
                    break;
            }
        }
    }

    public void Start()
    {
        throw new NotImplementedException();
    }

    public void SetAes(Aes aes)
    {
        _aes = aes;
    }
}