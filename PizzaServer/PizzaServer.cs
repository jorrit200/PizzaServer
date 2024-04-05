// See https://aka.ms/new-console-template for more information
using System.Text;
using System.Text.RegularExpressions;   
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Sockets;

namespace PizzaServer;

internal class PizzaServer
{
    
    const string eol = "\r\n";
    const string eot = "EOT;";
    private static Aes symmetricKey;
    public static void Main(string[] args)
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"),6789);
        tcpListener.Start();
        Console.WriteLine("server started");

        var myRSA = new RSACryptoServiceProvider();
        string publicKeyXML = myRSA.ToXmlString(false);
        
        var clientRSA = new RSACryptoServiceProvider();
        

        TcpClient tcpClient = tcpListener.AcceptTcpClient();
        Console.WriteLine("client connected");
        NetworkStream stream = tcpClient.GetStream();



        //enter to an infinite cycle to be able to handle every change in stream
        while (true) {
            while(tcpClient.Available < 3){}

            byte[] bytes = new byte[tcpClient.Available];

            stream.Read(bytes, 0, bytes.Length);

            String data = ByteToStringUtf(bytes);
            
            if (Regex.IsMatch(data, "^GET")) {
                Console.WriteLine("We are getting a GET request");
                string requestType = new Regex("Type: (.*)").Match(data).Groups[1].Value.Trim();
                string message = new Regex("Message: (.*)").Match(data).Groups[1].Value.Trim();
                string ClientsPublicKey = new Regex("Public-key: (.*)").Match(data).Groups[1].Value.Trim();
                
                byte[] response;
                bool encrypted = false;
                if (requestType == "request-public-key")
                {
                    clientRSA.FromXmlString(ClientsPublicKey);
                    symmetricKey = GenerateSymmetricKey();
                    symmetricKey.GenerateIV();

                    Console.WriteLine(BitConverter.ToString(symmetricKey.Key));
                    
                    var encryptedSymmetricKey = clientRSA.Encrypt(symmetricKey.Key, true);
                    
                    
                    response = StringToByteUtf("PIZZA/1.1 200 OK" + eol
                                             + "public-key: " + publicKeyXML + eol
                                             + "symmetric-key: " + ByteToStringBase64(encryptedSymmetricKey) + eot);
                }
                else if (requestType == "pizza")
                {
                    response = BuildResponse("now you're speaking my language", symmetricKey);
                    encrypted = true;

                    byte[] byte_array = StringToByteBase64(message);
                    byte[] decrypyed_message = myRSA.Decrypt(byte_array, true);
                    string straightDecrypt = ByteToStringBase64(decrypyed_message);
                    string utfDecrypt = ByteToStringUtf(decrypyed_message);
                }
                else
                {
                    response = StringToByteUtf("PIZZA/1.1 400 OK" + eol
                                                               + "AYO WTF"
                                                               + eol);
                }

                stream.Write(response, 0, response.Length);
            } else {
                Console.WriteLine("We dont have a GET request");
            }
        }
    }
    
    static byte[] StringToByteBase64(string string_to_encode)
    {
        return Convert.FromBase64String(string_to_encode);
    }
    
    static string ByteToStringBase64(byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }
    
    static byte[] StringToByteUtf(string string_to_encode)
    {
        return Encoding.UTF8.GetBytes(string_to_encode);
    }
    
    static string ByteToStringUtf(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }
    
    static Aes GenerateSymmetricKey()
    {
        Aes aes = Aes.Create();
        aes.GenerateKey();
        return aes;
    }
    
    static byte[] BuildResponse(string response, Aes key = null)
    {
        string header_protocol = "GET PIZZA/1.1" + eol;
        string header_IV = "IV: " + ByteToStringBase64(key.IV) + eol;
        string header_message = "Message: ";
        byte[] message_bytes = StringToByteUtf(response);
        byte[] header_bytes = StringToByteUtf(header_protocol + header_IV + header_message);
        byte[] encrypted_message = SymmetricEncrypt(message_bytes, key);
        
        string based_message = ByteToStringBase64(encrypted_message);
        byte[] final_message = StringToByteUtf(based_message + eot);
        byte[] bytes_to_send = header_bytes.Concat(final_message).ToArray();
        return bytes_to_send;

    }
    
    static byte[] SymmetricEncrypt(byte[] message, Aes key)
    {
        return key.EncryptCbc(message, key.IV);
    }
    
}