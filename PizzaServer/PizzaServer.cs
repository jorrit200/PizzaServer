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

            String data = Encoding.ASCII.GetString(bytes);
            
            if (Regex.IsMatch(data, "^GET")) {
                Console.WriteLine("We are getting a GET request");
                const string eol = "\r\n";
                const string eot = "EOT;";
                string requestType = new Regex("Type: (.*)").Match(data).Groups[1].Value.Trim();
                string content = new Regex("Message: (.*)").Match(data).Groups[1].Value.Trim();
                string ClientsPublicKey = new Regex("Public-key: (.*)").Match(data).Groups[1].Value.Trim();
                
                string message = content.Split(eot)[0];
                string digsig = content.Split(eot)[1];
                
                Console.WriteLine("Request type = " + requestType);
                Console.WriteLine("Message: " + message);
                Console.WriteLine("Digsig: " + digsig);

                byte[] response;
                bool encrypted = false;
                if (requestType == "request-public-key")
                {
                    clientRSA.FromXmlString(ClientsPublicKey);
                    
                    response = Encoding.UTF8.GetBytes("PIZZA/1.1 200 OK" + eol
                        + "public-key: " + publicKeyXML     
                    + eol);
                    Console.WriteLine("MY-public-key: " + publicKeyXML);
                    Console.WriteLine("Client-public-key: " + ClientsPublicKey);
                }
                else if (requestType == "pizza")
                {
                    response = Encoding.UTF8.GetBytes("PIZZA/1.1 400 OK" + eol
                         + "this guy gets no bitches" 
                         + eol);
                    response = clientRSA.Encrypt(response, true);
                    encrypted = true;
                    
                    byte[] decrypyed_message =
                        myRSA.Decrypt(Encoding.UTF8.GetBytes(message), true);
                    
                }
                else
                {
                    response = Encoding.UTF8.GetBytes("PIZZA/1.1 400 OK" + eol
                        + "AYO WTF"
                        + eol);
                    response = clientRSA.Encrypt(response, true);
                    encrypted = true;
                }



                if (encrypted){
                    String responseStr = Encoding.UTF8.GetString(response);
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
}