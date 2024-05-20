using System.Security.Cryptography;
using PizzaServer.Responses;

namespace PizzaServer.Observers;

/// <summary>
/// Implement this interface to allow your observer to attach to a <see cref="IServerSubject"/>
/// This observer is now responsible for handeling all requests of a certain request type.
/// <br/>
/// <see cref="Update"/> is called whenver the assigened request type has been pushed by a client.
/// You are responsible for reading the data, and sending an according response to the given <see cref="IResponse"/> obejct
/// </summary>
/// <warning>
/// Data send to and from this observer is not encrypted. If you dont know what this means, you are likely looking for <see cref="ISocketObserverRequireAes"/>
/// </warning>
public interface ISocketObserver
{
    /// <summary>
    /// This method will get called on a new request
    /// </summary>
    /// <param name="data">The raw request coming in</param>
    /// <param name="response">The object to send your response to</param>
    void Update(string data, IResponse response);
}

/// <summary>
/// Implement this interface to use asymmetrical encrypted communication on a request type.
/// Also requires you to produce an Aes object, so this interface basically only exists for the <see cref="KeyExchangeObserver"/>
/// </summary>
/// <seealso cref="ISocketObserver"/>
public interface ISocketObserverRequireRsa : ISocketObserver
{
    /// <summary>
    /// This method will get called on a new request, if RSA is available.
    /// </summary>
    /// <param name="data">The raw request comming in</param>
    /// <param name="response">The object to send your response to</param>
    /// <param name="rsa">The server's RSA object</param>
    /// <param name="aesReceiver">object to potentially send a symmetric key to</param>
    void Update(string data, IResponse response, RSACryptoServiceProvider rsa,
        IHaveAes aesReceiver);
}

/// <summary>
/// Implement this interface for symmetrically encrypted communication on a request type.
/// </summary>
public interface ISocketObserverRequireAes : ISocketObserver
{
    /// <summary>
    /// This method will get called on a new request, if Aes is available.
    /// </summary>
    /// <param name="data">The raw request</param>
    /// <param name="response">The object to send your response to.</param>
    /// <param name="aes">The symmetric key object, to encrypt, and decrypt data with</param>
    void Update(string data, IResponse response, Aes aes);
}

/// <summary>
/// Implement this Interface to build a  server!
/// </summary>
public interface IServerSubject
{
    /// <summary>
    /// Attach a <see cref="ISocketObserver"/> to your server, listening to a specific request type
    /// Your observer is responsible for handeling requests from this type.
    /// </summary>
    /// <param name="socketObserver"> The observer that will be handling a certain request type</param>
    /// <param name="requestType">The request type to listen for</param>
    void Attach(ISocketObserver socketObserver, string requestType);
    
    void Detach(ISocketObserver socketObserver, string requestType);
    
    /// <summary>
    /// Notify the correct observer with the incoming message
    /// </summary>
    /// <implementation>
    /// Identify the correct observer to call, based on observer-type and request-type. Then call its update method with the required parameters.
    /// </implementation>
    /// <param name="requestType"></param>
    /// <param name="message"></param>
    /// <param name="response"></param>
    void Notify(string requestType, string message, IResponse response);
    
    /// <summary>
    /// Start the server. Godspeed.
    /// </summary>
    void Start();
}