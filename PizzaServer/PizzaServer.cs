﻿// See https://aka.ms/new-console-template for more information
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
        
        string path = @"..\..\..\keys.xml";
        string xmlString = File.ReadAllText(path);
        RSAParameters rsaParameters;
        using (StringReader reader = new StringReader(xmlString))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(RSAParameters));
            rsaParameters = (RSAParameters)serializer.Deserialize(reader);
        }

        var myRSA = new RSACryptoServiceProvider();
        myRSA.ImportParameters(rsaParameters);
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
                string requestType = new Regex("Type: (.*)").Match(data).Groups[1].Value.Trim();
                string message = new Regex("Message: (.*)").Match(data).Groups[1].Value.Trim();
                string ClientsPublicKey = new Regex("Public-key: (.*)").Match(data).Groups[1].Value.Trim();
                
                Console.WriteLine("Request type = " + requestType);

                byte[] response;
                if (requestType == "request-public-key")
                {
                    clientRSA.FromXmlString(ClientsPublicKey);
                    
                    response = Encoding.ASCII.GetBytes("PIZZA/1.1 200 OK" + eol
                        + "public-key: " + publicKeyXML     
                    + eol);    
                }
                else if (requestType == "pizza")
                {
                    response = Encoding.ASCII.GetBytes("PIZZA/1.1 400 OK" + eol
                         + "AYO WTF" 
                         + eol);
                    response = clientRSA.Encrypt(response, true);
                    
                    byte[] decrypyed_message =
                        myRSA.Decrypt(Encoding.ASCII.GetBytes(message), true);
                    
                }
                else
                {
                    response = Encoding.ASCII.GetBytes("PIZZA/1.1 400 OK" + eol
                        + "AYO WTF"
                        + eol);
                    response = clientRSA.Encrypt(response, true);
                }
                
                
                
                String responseStr = Encoding.ASCII.GetString(response);
                Console.WriteLine("resSize: " + responseStr.Length);

                stream.Write(response, 0, response.Length);
            } else {
                Console.WriteLine("We dont have a GET request");
            }
        }
    }
}