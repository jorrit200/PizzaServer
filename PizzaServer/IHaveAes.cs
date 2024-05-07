using System.Security.Cryptography;

namespace PizzaServer;

public interface IHaveAes
{
    public void SetAes(Aes aes);
}