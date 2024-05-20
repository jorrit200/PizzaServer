# Een server bouwen

Een [Observer](../Observers/ISocketObserver.cs) werkt op meerdere server types. Het is dus niet de taak van de server om de verschillende request af te vangen, maar om de onderliggende communicatie techniek af te handelen.

Het is de taak van de server om het ruwe bericht als een string aand Observer te geven, en een [IResponse](../Responses/IResponse.cs) mee te geven waar de observer zijn antwoord heen kan sturen.

Een server implementeerd dus een netwerk protocol ([TCP](TcpSubjectServer.cs)/[UDP](UdpSubjectServer.cs)). Maar niet het communicatie protocol (Beide servers en subjects gaan uit van het pizza protocol).

Communicatie hoeft echter niet over een netwerk te gaan. Laten we een "server" bouwen om onze Observers te testen.
In dit document wordt stap voor stap de [TestSubjectServer](TestSubjectServer.cs) gebouwd.

## 1. De classes
Een SubjectServer implementeert [IServerSubject](../Observers/ISocketObserver.cs) en [IHaveAes](../IHaveAes.cs) (als het nodig is om observers te attachen die AES vereisen of aanmaken, hierover later meer).

Laten we de class `TestSubjectServer` aanmaken, de interfaces inhereten, en members implementeren.A

```csharp
// TestSubjectServer.cs
public class TestSubjectServer: IServerSubject, IHaveAes 
{
    public void Attach(ISocketObserver socketObserver, string requestType);
    public void Detach(ISocketObserver socketObserver, string requestType);
    
    public void Notify(string requestType, string message, IResponse response);
    
    public void Start();
    
    public void SetAes(Aes aes);
}   

```
En een `TestResponse` class
```csharp
// TestResponse.cs
public class TestResponse: IResponse
{
    public void Send(byte[] message)
    {
        Console.WriteLine(message);
    }
}
```
De send method definieert wat er gebeurt met de message die een observer terug stuurt. Aangezien dit een lokale test server wordt, willen we dit voor nu gewoon naar de console schrijven.

## 2. Server verantwoordelijkheden
De server heeft een aantal methods moeten implementeren, deze methods leggen verantwoordelijheid af bij de server.

### 2.1 Observers collecten
`Attach` en `Detatch` moeten bijvoorbeeld een collectie met observers bijhouden, die te koppelen zijn aan een request type.
Laten we de volgende collectie gebruiken:
```csharp
// TestSubjectServer.cs
private readonly Dictionary<string, List<ISocketObserver>> _observers = new();
```
En de methods implementeren:
```csharp
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
```
De server houdt nu een dictonary mij met request-types als keys, en arrays met observers als values. In theorie is het dus mogelijk om meerdere observers aan een request te koppelen.

### 2.2 Notify
De notify functie stuurt elk binnenkomend bericht naar