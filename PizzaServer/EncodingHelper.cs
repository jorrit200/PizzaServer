using System.Security.Cryptography;
using System.Text;

namespace PizzaServer;

/// <summary>
/// Encoding and decoding messsages to various models is what this class is for.
/// Also helps with encryption and message builing.
/// </summary>
public static class EncodingHelper
{
    public static byte[] StringToByteBase64(string stringToEncode)
    {
        return Convert.FromBase64String(stringToEncode);
    }
    
    public static string ByteToStringBase64(byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }
    
    public static byte[] StringToByteUtf(string stringToEncode)
    {
        return Encoding.UTF8.GetBytes(stringToEncode);
    }
    
    public static string ByteToStringUtf(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }
    
    public static Aes GenerateSymmetricKey()
    {
        var aes = Aes.Create();
        aes.GenerateKey();
        return aes;
    }
    
    public static byte[] SymmetricEncrypt(byte[] message, Aes key)
    {
        return key.EncryptCbc(message, key.IV);
    }
    
    public static byte[] SymmetricDecrypt(byte[] message, Aes key)
    {
        return key.DecryptCbc(message, key.IV);
    }
    
    public static string DecodeMessage(string message, Aes aes, string iv)
    {
        var encryptedBytes = StringToByteBase64(message);
        var ivBytes = StringToByteBase64(iv);
        Console.WriteLine(BitConverter.ToString(ivBytes));
        Console.WriteLine(BitConverter.ToString(aes.IV));
        aes.IV = ivBytes;
        var decryptedBytes = SymmetricDecrypt(encryptedBytes, aes);
        var decodedMessage = ByteToStringUtf(decryptedBytes);
        return decodedMessage;
    }
    
    public static byte[] BuildResponse(string response, Aes key)
    {
        const string headerProtocol = "GET PIZZA/1.1" + PizzaServer.Eol;
        var headerIv = "IV: " + ByteToStringBase64(key.IV) + PizzaServer.Eol;
        var headerMessage = "Message: ";
        var messageBytes = StringToByteUtf(response);
        var headerBytes = StringToByteUtf(headerProtocol + headerIv + headerMessage);
        var encryptedMessage = SymmetricEncrypt(messageBytes, key);
        
        var basedMessage = ByteToStringBase64(encryptedMessage);
        var finalMessage = StringToByteUtf(basedMessage + PizzaServer.Eot);
        var bytesToSend = headerBytes.Concat(finalMessage).ToArray();
        return bytesToSend;

    }
}