# Een server bouwen

Een [Observer](../Observers/ISocketObserver.cs) werkt op meerdere server types. Het is dus niet de taak van de server om de verschillende request af te vangen, maar om de onderliggende communicatie techniek af te handelen.

Het is de taak van de server om het ruwe bericht als een string aan de Observer te geven, en een [IResponse](../Responses/IResponse.cs) mee te geven waar de observer zijn antwoord heen kan sturen.

Een server implementeerd dus een netwerk protocol ([TCP](TcpSubjectServer.cs)/[UDP](UdpSubjectServer.cs)), maar niet het communicatie protocol (Beide servers en subjects gaan uit van het pizza protocol).

Communicatie hoeft echter niet over een netwerk te gaan. Laten we een "server" bouwen om onze Observers te testen.
In dit document wordt stap voor stap de [TestSubjectServer](TestSubjectServer.cs) gebouwd.

## 1. De classes
Een SubjectServer implementeert [IServerSubject](../Observers/ISocketObserver.cs) en [IHaveAes](../IHaveAes.cs) (als het nodig is om observers te attachen die AES vereisen of aanmaken, hierover later meer).

Laten we de class `TestSubjectServer` aanmaken, de interfaces inhereten, en members implementeren.

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

### 2.2 Keys
De server moet zijn eigen keys bijhouden, om Observers af te kunnen gebruiken die RSA of AES keys gebruiken (de observers die ISocketObserverRequireRSa of IsocketObserverRequireAes, implementeren). Aangezien elke observer die we hebben gemaakt een sleutel verijst (omdat alle data versleuteld moet worden als verijste van de opdracht) zal onze test server dit ook moetnen doen.

De server kan gemakkelijk zijn eigen RSA key aanmaken, en de AES key zal gelverd worden door [KeyExcangeObserver](../Observers/KeyExchangeObserver.cs). De server hoeft dus alleen een RSA object aan te maken, en een field hebben om een Aes key te houden.
```csharp
private readonly RSACryptoServiceProvider _rsa = new();
private Aes? _aes;
```
Het RSA object is readonly omdat de key niet gecycled word, en dus nooit verandert hoeft te worden.  
Het AES object kan null zijn, omdat de server voor zijn eerste request nog geen AES key zal hebben (de krijgt hij immers van de key exchange request)

Laten we de `SetAes` method implementeren, die we eerder hebben geërft hebben van `IHaveAes`

```csharp
public void SetAes(Aes aes)
{
    _aes = aes;
}
```

### 2.3 Notify
De notify method stuurt elk binnenkomend bericht naar de correcte observer. Dit doet hij door de request type te matchen met het type in de dictionary. Ook word er op basis van de geimplementeerde Interface gekeken of de server kan voldoen aan de eis die deze Interface stelt, bijvoorbeeld AES of RSA (hoewel RSA te garanderen valt). Dit zelfde princiepe kan ook gebruikt worden voor het vereisen van een database connectie bijvoorbeeld.
Hier volgt de implementatie:
```csharp
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
```

# 3. Het starten van de server
Tot nu toe hebben we alle methods geimplementeerd die onze Interfaces vereisen, bahlve de `Start()` method. De start methode opent een endpoint om te gaan praten met de server, en start vaak een eindelose loop om te luisteren naar clients.  
Tot dusver zijn we alleen nog maar abstract bezig geweest met de logica van onze server, maar nu word het tijd om de daadwerkelijke onderliggende communicatie te gaan implementeren.
