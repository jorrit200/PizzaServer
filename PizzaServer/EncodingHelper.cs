using System.Text;
using System.Text.RegularExpressions;   
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Sockets;

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
    
    public static Aes GenerateSymmetricKey()
    {
        Aes aes = Aes.Create();
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
        byte[] encryptedBytes = EncodingHelper.StringToByteBase64(message);
        byte[] ivBytes = EncodingHelper.StringToByteBase64(iv);
        Console.WriteLine(BitConverter.ToString(ivBytes));
        Console.WriteLine(BitConverter.ToString(aes.IV));
        aes.IV = ivBytes;
        byte[] decryptedBytes = EncodingHelper.SymmetricDecrypt(encryptedBytes, aes);
        string decodedMessage = EncodingHelper.ByteToStringUtf(decryptedBytes);
        return decodedMessage;
    }
    
    public static byte[] BuildResponse(string response, Aes key = null)
    {
        string headerProtocol = "GET PIZZA/1.1" + PizzaServer.Eol;
        string headerIv = "IV: " + EncodingHelper.ByteToStringBase64(key.IV) + PizzaServer.Eol;
        string headerMessage = "Message: ";
        byte[] messageBytes = EncodingHelper.StringToByteUtf(response);
        byte[] headerBytes = EncodingHelper.StringToByteUtf(headerProtocol + headerIv + headerMessage);
        byte[] encryptedMessage = EncodingHelper.SymmetricEncrypt(messageBytes, key);
        
        string basedMessage = EncodingHelper.ByteToStringBase64(encryptedMessage);
        byte[] finalMessage = EncodingHelper.StringToByteUtf(basedMessage + PizzaServer.Eot);
        byte[] bytesToSend = headerBytes.Concat(finalMessage).ToArray();
        return bytesToSend;

    }
}