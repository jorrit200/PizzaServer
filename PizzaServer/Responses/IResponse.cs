namespace PizzaServer.Responses;

/// <summary>
/// Send your response to this object, when you recieve in in Update as an observer
/// </summary>>
/// <implementation>
/// Implement this interface, and push this object to the Update method of your <see cref="IServerSubject"/>
/// </implementation>
public interface IResponse
{
    /// <summary>
    /// Call this method to send your reposne
    /// </summary>
    /// <implementation>
    /// The observer calls this method with its response. Implement sending the response back to the client.
    /// </implementation>
    public void Send(byte[] message);
}