using System.Text;
using System.Text.RegularExpressions;   
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Sockets;

namespace PizzaServer;

public class EncoderHelper
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
}