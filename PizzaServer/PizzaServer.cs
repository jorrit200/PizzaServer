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
    public static void Main(string[] args)
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"),6789);
        tcpListener.Start();
        Console.WriteLine("server started");

        var myRSA = new RSACryptoServiceProvider();
        string publicKeyXML = myRSA.ToXmlString(false);
        
        Console.WriteLine("Key Size: " + myRSA.KeySize);
        
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

                Console.WriteLine(data);
                string requestType = new Regex("Type: (.*)").Match(data).Groups[1].Value.Trim();
                string message = new Regex("Message: (.*)").Match(data).Groups[1].Value.Trim();
                string ClientsPublicKey = new Regex("Public-key: (.*)").Match(data).Groups[1].Value.Trim();
                
                byte[] response;
                bool encrypted = false;
                if (requestType == "request-public-key")
                {
                    clientRSA.FromXmlString(ClientsPublicKey);
                    
                    response = StringToByteUtf("PIZZA/1.1 200 OK" + eol
                                                                         + "public-key: " + publicKeyXML     
                                                                         + eol);
                    Console.WriteLine("MY-public-key: " + publicKeyXML);
                    Console.WriteLine("Client-public-key: " + ClientsPublicKey);
                }
                else if (requestType == "pizza")
                {
                    response = buildResponse("now you're speaking my language", clientRSA);
                    encrypted = true;

                    byte[] byte_array = StringToByteBase64(message);
                    Console.WriteLine(byte_array.Length);
                    Console.WriteLine(BitConverter.ToString(byte_array));
                    byte[] decrypyed_message = myRSA.Decrypt(byte_array, true);
                    string straightDecrypt = ByteToStringBase64(decrypyed_message);
                    string utfDecrypt = ByteToStringUtf(decrypyed_message);
                    Console.WriteLine("Base64: " + straightDecrypt);
                    Console.WriteLine("UTF8" + utfDecrypt);
                }
                else
                {
                    response = StringToByteUtf("PIZZA/1.1 400 OK" + eol
                                                               + "AYO WTF"
                                                               + eol);
                }



                if (encrypted){
                    String responseStr = ByteToStringUtf(response);
                    Console.WriteLine("resSize: " + response.Length);
                    // String decodedRESP = Encoding.UTF8.GetString(myRSA.Decrypt(response, true));
                    Console.WriteLine("res: " + responseStr);
                    string hex = BitConverter.ToString(response);
                    Console.WriteLine("Hex: " + hex);
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
    
    static byte[] buildResponse(string response, RSACryptoServiceProvider rsa = null)
    {
        string header_protocol = "GET PIZZA/1.1" + eol;
        string header_message = "Message: ";
        string digsig = "its me btw";
        
        byte[] message_bytes = StringToByteUtf(response + eot + digsig);
        byte[] header_bytes = StringToByteUtf(header_protocol + header_message);
        byte[] encrypted_message = rsa.Encrypt(message_bytes, true);
        string based_message = ByteToStringBase64(encrypted_message);
        byte[] final_message = StringToByteUtf(based_message);
        byte[] bytes_to_send = header_bytes.Concat(final_message).ToArray();
        return bytes_to_send;

    }
}