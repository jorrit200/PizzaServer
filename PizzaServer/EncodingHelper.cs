using System.Security.Cryptography;
using System.Text;

namespace PizzaServer;

public class EncodingHelper
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
    
    public static Aes? GenerateSymmetricKey()
    {
        Aes? aes = Aes.Create();
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
        byte[] encryptedBytes = StringToByteBase64(message);
        byte[] ivBytes = StringToByteBase64(iv);
        Console.WriteLine(BitConverter.ToString(ivBytes));
        Console.WriteLine(BitConverter.ToString(aes.IV));
        aes.IV = ivBytes;
        byte[] decryptedBytes = SymmetricDecrypt(encryptedBytes, aes);
        string decodedMessage = ByteToStringUtf(decryptedBytes);
        return decodedMessage;
    }
    
    public static byte[] BuildResponse(string response, Aes key)
    {
        string headerProtocol = "GET PIZZA/1.1" + PizzaServer.Eol;
        string headerIv = "IV: " + ByteToStringBase64(key.IV) + PizzaServer.Eol;
        string headerMessage = "Message: ";
        byte[] messageBytes = StringToByteUtf(response);
        byte[] headerBytes = StringToByteUtf(headerProtocol + headerIv + headerMessage);
        byte[] encryptedMessage = SymmetricEncrypt(messageBytes, key);
        
        string basedMessage = ByteToStringBase64(encryptedMessage);
        byte[] finalMessage = StringToByteUtf(basedMessage + PizzaServer.Eot);
        byte[] bytesToSend = headerBytes.Concat(finalMessage).ToArray();
        return bytesToSend;

    }
}