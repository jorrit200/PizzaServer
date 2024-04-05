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
    public const string Eol = "\r\n";
    public const string Eot = "EOT;";
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
            TcpSubjectServer tcpSubjectServer = new TcpSubjectServer(6789);
            tcpSubjectServer.Attach(new KeyExchangeObserver(), "request-symmetric-key");
            tcpSubjectServer.Attach(new PizzaObserver(_menu, _toppings), "pizza");
            tcpSubjectServer.Attach(new MenuObserver(_menu), "menu");


            tcpSubjectServer.Start();
        }
        else
        {
            Console.WriteLine("UDP");
            UdpClient udpClient = new UdpClient(6789);
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                byte[] receivedBytes = udpClient.Receive(ref remoteIpEndPoint);
                string receivedData = EncodingHelper.ByteToStringUtf(receivedBytes);
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
                        _symmetricKey = EncodingHelper.GenerateSymmetricKey();
                        _symmetricKey.GenerateIV();

                        Console.WriteLine(BitConverter.ToString(_symmetricKey.Key));

                        var encryptedSymmetricKey = clientRsa.Encrypt(_symmetricKey.Key, true);
                        
                        response = EncodingHelper.StringToByteUtf("PIZZA/1.1 200 OK" + Eol
                          + "public-key: " + publicKeyXml + Eol
                          + "symmetric-key: " +
                          EncodingHelper.ByteToStringBase64(encryptedSymmetricKey) + Eot);
                        udpClient.Send(response, response.Length, remoteIpEndPoint);

                    }
                }
            }
        }
    }
    
    public static void SetSymmetricKey(Aes key)
    {
        _symmetricKey = key;
    }

}