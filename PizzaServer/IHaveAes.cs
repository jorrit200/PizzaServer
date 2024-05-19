using System.Security.Cryptography;

namespace PizzaServer;

/// <summary>
/// Implement this interface to allow setting the aes object by a <see cref="Observers.ISocketObserverRequireRsa"/>
/// </summary>
public interface IHaveAes
{
    public void SetAes(Aes aes);
}