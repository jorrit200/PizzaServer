using System.Security.Cryptography;
using System.Text.RegularExpressions;
using PizzaServer.Responses;

namespace PizzaServer.Observers;

/// <summary>
/// The observer responsible for handling the exchange of the symmetric keys, by merits of asymmetric keys.
/// </summary>
public partial class KeyExchangeObserver: ISocketObserverRequireRsa
{
    public void Update(string data, IResponse response, RSACryptoServiceProvider rsa, IHaveAes aesReceiver)
    {
        var clientsPublicKey = MyRegex().Match(data).Groups[1].Value.Trim();
        
        var clientRsa = new RSACryptoServiceProvider();
        clientRsa.FromXmlString(clientsPublicKey);
        var symmetricKey = EncodingHelper.GenerateSymmetricKey();
        aesReceiver.SetAes(symmetricKey);
        symmetricKey.GenerateIV();
        
        var encryptedSymmetricKey = clientRsa.Encrypt(symmetricKey.Key, true);
        var responseMessage = EncodingHelper.StringToByteUtf(
            $"PIZZA/1.1 200 OK{PizzaServer.Eol}public-key: {rsa.ToXmlString(false)}{PizzaServer.Eol}" +
            $"symmetric-key: {EncodingHelper.ByteToStringBase64(encryptedSymmetricKey)}{PizzaServer.Eot}");
        
        response.Send(responseMessage);
    }

    public void Update(string data, IResponse response)
    {
        throw new Exception("I need RSA to work!");
    }

    [GeneratedRegex("Public-key: (.*)")]
    private static partial Regex MyRegex();
}