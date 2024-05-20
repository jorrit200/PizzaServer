using System.Security.Cryptography;
using PizzaServer.Observers;
using PizzaServer.Responses;

namespace PizzaServer.Servers;

public class TestSubjectServer: IServerSubject, IHaveAes 
{
    private readonly Dictionary<string, List<ISocketObserver>> _observers = new();
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
        throw new NotImplementedException();
    }

    public void Start()
    {
        throw new NotImplementedException();
    }

    public void SetAes(Aes aes)
    {
        throw new NotImplementedException();
    }
}