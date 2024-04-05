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
    const string Eol = "\r\n";
    const string Eot = "EOT;";
    private static Aes _symmetricKey;
    private static string protocol = "TCP";
    
    
    private static readonly Dictionary<string, int> _menu = new()
    {
        { "Margherita", 1000 },
        { "Tonno", 1250 },
        { "Calzone", 1300 }
    };

    private static readonly Dictionary<string, int> _toppings = new()
    {
        { "Mozzarella", 50 },
        { "Parmezaan", 50 },
        { "Gorgonzola", 50 },
        { "Cheddar", 50 },
        { "Ui", 50 },
        { "Paprika", 50 },
        { "Olijven", 50 },
        { "Ansjovis", 50 },
        { "Tonijn", 50 },
        { "Salami", 50 },
        { "Ham", 50 },
        { "Kip", 50 },
        { "Rundergehakt", 50 },
        { "Knoflook", 50 },
        { "Pepers", 50 },
        { "Champignons", 50 },
        { "Artisjokken", 50 },
        { "Spinazie", 50 },
        { "Rucola", 50 },
        { "Zongedroogde tomaten", 50 },
        { "Pijnboompitten", 50 },
        { "Kappertjes", 50 },
        { "Pesto", 50 },
        { "Barbecuesaus", 50 },
        { "Tomatensaus", 50 },
        { "Olijfolie", 50 },
        { "Balsamico", 50 },
        { "Honing", 50 },
        { "Knoflooksaus", 50 },
        { "Mayonaise", 50 },
        { "Ketchup", 50 },
        { "Sambal", 50 },
        { "Kerriesaus", 50 },
        { "Pindasaus", 50 },
        { "Satésaus", 50 },
        { "Guacamole", 50 },
        { "Zure room", 50 },
        { "Mosterd", 50 },
        { "Ketjap", 50 },
        { "Sojasaus", 50 },
        { "Worcestersaus", 50 },
        { "Tabasco", 50 },
        { "Sriracha", 50 }
    };
    
    public static void Main(string[] args)
    {
        Pizza pizza = new Pizza("Margherita", _menu).WithTopping("Tabasco", _toppings)
            .WithTopping("Mozzarella", _toppings);
        Console.WriteLine(pizza.GetPrice());
        
        TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"),6789);
        

        var myRsa = new RSACryptoServiceProvider();
        string publicKeyXml = myRsa.ToXmlString(false);
        
        var clientRsa = new RSACryptoServiceProvider();


        if (protocol == "TCP")
        {
            tcpListener.Start();
            Console.WriteLine("server started");
            TcpClient tcpClient = tcpListener.AcceptTcpClient();
            Console.WriteLine("client connected");
            NetworkStream stream = tcpClient.GetStream();



            //enter to an infinite cycle to be able to handle every change in stream
            while (true)
            {
                while (tcpClient.Available < 3)
                {
                }

                byte[] bytes = new byte[tcpClient.Available];

                stream.Read(bytes, 0, bytes.Length);

                String data = ByteToStringUtf(bytes);

                if (Regex.IsMatch(data, "^GET"))
                {
                    Console.WriteLine("We are getting a GET request");
                    string requestType = new Regex("Type: (.*)").Match(data).Groups[1].Value.Trim();
                    string message = new Regex("Message: (.*)").Match(data).Groups[1].Value.Trim();
                    string clientsPublicKey = new Regex("Public-key: (.*)").Match(data).Groups[1].Value.Trim();
                    string clientsIv = new Regex("IV: (.*)").Match(data).Groups[1].Value.Trim();

                    byte[] response;
                    bool encrypted = false;
                    if (requestType == "request-public-key")
                    {
                        clientRsa.FromXmlString(clientsPublicKey);
                        _symmetricKey = GenerateSymmetricKey();
                        _symmetricKey.GenerateIV();

                        Console.WriteLine(BitConverter.ToString(_symmetricKey.Key));

                        var encryptedSymmetricKey = clientRsa.Encrypt(_symmetricKey.Key, true);


                        response = StringToByteUtf("PIZZA/1.1 200 OK" + Eol
                                                                      + "public-key: " + publicKeyXml + Eol
                                                                      + "symmetric-key: " +
                                                                      ByteToStringBase64(encryptedSymmetricKey) + Eot);
                    }
                    else if (requestType == "pizza")
                    {
                        string decodedMessage = DecodeMessage(message, _symmetricKey, clientsIv);

                        response = BuildResponse("now you're speaking my language", _symmetricKey);
                    }

                    else if (requestType == "menue")
                    {
                        response = BuildResponse(DictionaryToString(_menu), _symmetricKey);
                    }
                    else
                    {
                        response = StringToByteUtf("PIZZA/1.1 400 OK" + Eol
                                                                      + "AYO WTF"
                                                                      + Eol);
                    }

                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    Console.WriteLine("We dont have a GET request");
                }
            }
        }
        else
        {
            Console.WriteLine("UDP");
            UdpClient udpClient = new UdpClient(6789);
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                byte[] receivedBytes = udpClient.Receive(ref remoteIpEndPoint);
                string receivedData = ByteToStringUtf(receivedBytes);
                Console.WriteLine(receivedData);
                if (Regex.IsMatch(receivedData, "^GET"))
                {
                    string requestType = new Regex("Type: (.*)").Match(receivedData).Groups[1].Value.Trim();
                    string message = new Regex("Message: (.*)").Match(receivedData).Groups[1].Value.Trim();
                    string clientsPublicKey = new Regex("Public-key: (.*)").Match(receivedData).Groups[1].Value.Trim();
                    string clientsIv = new Regex("IV: (.*)").Match(receivedData).Groups[1].Value.Trim();

                    byte[] response;
                    bool encrypted = false;
                    if (requestType == "request-public-key")
                    {
                        clientRsa.FromXmlString(clientsPublicKey);
                        _symmetricKey = GenerateSymmetricKey();
                        _symmetricKey.GenerateIV();

                        Console.WriteLine(BitConverter.ToString(_symmetricKey.Key));

                        var encryptedSymmetricKey = clientRsa.Encrypt(_symmetricKey.Key, true);
                        
                        response = StringToByteUtf("PIZZA/1.1 200 OK" + Eol
                          + "public-key: " + publicKeyXml + Eol
                          + "symmetric-key: " +
                          ByteToStringBase64(encryptedSymmetricKey) + Eot);
                        udpClient.Send(response, response.Length, remoteIpEndPoint);

                    }
                }
            }
        }
    }
    
    static byte[] StringToByteBase64(string stringToEncode)
    {
        return Convert.FromBase64String(stringToEncode);
    }
    
    static string ByteToStringBase64(byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }
    
    static byte[] StringToByteUtf(string stringToEncode)
    {
        return Encoding.UTF8.GetBytes(stringToEncode);
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
    
    static string DecodeMessage(string message, Aes aes, string iv)
    {
        byte[] encryptedBytes = StringToByteBase64(message);
        byte[] ivBytes = StringToByteBase64(iv);
        aes.IV = ivBytes;
        byte[] decryptedBytes = SymmetricDecrypt(encryptedBytes, aes);
        string decodedMessage = ByteToStringUtf(decryptedBytes);
        return decodedMessage;
    }
    
    static byte[] BuildResponse(string response, Aes key = null)
    {
        string headerProtocol = "GET PIZZA/1.1" + Eol;
        string headerIv = "IV: " + ByteToStringBase64(key.IV) + Eol;
        string headerMessage = "Message: ";
        byte[] messageBytes = StringToByteUtf(response);
        byte[] headerBytes = StringToByteUtf(headerProtocol + headerIv + headerMessage);
        byte[] encryptedMessage = SymmetricEncrypt(messageBytes, key);
        
        string basedMessage = ByteToStringBase64(encryptedMessage);
        byte[] finalMessage = StringToByteUtf(basedMessage + Eot);
        byte[] bytesToSend = headerBytes.Concat(finalMessage).ToArray();
        return bytesToSend;

    }
    
    static byte[] SymmetricEncrypt(byte[] message, Aes key)
    {
        return key.EncryptCbc(message, key.IV);
    }
    
    static byte[] SymmetricDecrypt(byte[] message, Aes key)
    {
        return key.DecryptCbc(message, key.IV);
    }
    
    static string DictionaryToString(Dictionary<string, int> dictionary)
    {
        string result = "";
        foreach (var item in dictionary)
        {
            result += item.Key + ": " + item.Value + Eol;
        }
        return result;
    }
}