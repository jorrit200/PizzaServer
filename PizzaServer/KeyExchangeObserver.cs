using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace PizzaServer;

public class KeyExchangeObserver: ISocketObserverRequireRsa
{
    public void Update(string requestType, string data, NetworkStream stream, RSACryptoServiceProvider rsa)
    {
        if (requestType == "request-public-key")
        {
            string message = new Regex("Message: (.*)").Match(data).Groups[1].Value.Trim();
            string clientsPublicKey = new Regex("Public-key: (.*)").Match(data).Groups[1].Value.Trim();
            
            RSACryptoServiceProvider clientRsa = new RSACryptoServiceProvider();
            clientRsa.FromXmlString(clientsPublicKey);
            Aes symmetricKey = EncoderHelper.GenerateSymmetricKey();
            symmetricKey.GenerateIV();
            
            PizzaServer.SetSymmetricKey(symmetricKey);
            
            var encryptedSymmetricKey = clientRsa.Encrypt(symmetricKey.Key, true);
            ;
            byte[] response = EncoderHelper.StringToByteUtf("PIZZA/1.1 200 OK" + PizzaServer.Eol
                                                          + "public-key: " + rsa.ToXmlString(false) + PizzaServer.Eol
                                                          + "symmetric-key: " +
                                                          EncoderHelper.ByteToStringBase64(encryptedSymmetricKey) + PizzaServer.Eot);
            
            stream.Write(response, 0, response.Length);
        }
        else
        {
            throw new Exception("Invalid request type");
        }
    }

    public void Update(string requestType, string data, NetworkStream stream)
    {
        throw new Exception("I need RSA to work!");
    }
}