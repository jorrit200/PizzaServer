using System.Security.Cryptography;
using System.Text.RegularExpressions;
using PizzaServer.Responses;

namespace PizzaServer.Observers;

public class KeyExchangeObserver: ISocketObserverRequireRsa
{
    public void Update(string requestType, string data, IResponse response, RSACryptoServiceProvider rsa, IHaveAes server)
    {
        string clientsPublicKey = new Regex("Public-key: (.*)").Match(data).Groups[1].Value.Trim();
        
        RSACryptoServiceProvider clientRsa = new RSACryptoServiceProvider();
        clientRsa.FromXmlString(clientsPublicKey);
        Aes? symmetricKey = EncodingHelper.GenerateSymmetricKey();
        server.SetAes(symmetricKey);
        symmetricKey.GenerateIV();
        
        var encryptedSymmetricKey = clientRsa.Encrypt(symmetricKey.Key, true);
        byte[] responseMessage = EncodingHelper.StringToByteUtf("PIZZA/1.1 200 OK" + PizzaServer.Eol
          + "public-key: " + rsa.ToXmlString(false) + PizzaServer.Eol
          + "symmetric-key: " +
          EncodingHelper.ByteToStringBase64(encryptedSymmetricKey) + PizzaServer.Eot);
        
        response.Send(responseMessage);
    }

    public void Update(string requestType, string data, IResponse response)
    {
        throw new Exception("I need RSA to work!");
    }
}