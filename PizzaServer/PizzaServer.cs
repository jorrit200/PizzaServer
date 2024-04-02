// See https://aka.ms/new-console-template for more information
using System.Text;
using System.Text.RegularExpressions;   
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;

namespace PizzaServer;

using System.Net.Sockets;

internal class PizzaServer
{
    public static void Main(string[] args)
    {
        string PRIVATEKEY = "MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDNf3yfLrlCM9ps\nbQ1Zy2p6mqcPIserXpq111YesXR3tBjS1EBDxDIBwC1Ub70B7Ov14PKUveSsVJgc\nEbaxSUEr1+0CbQIf6idRpiCvg5TZNK9/ywdccun4W1HlLfym/nBRdFiJAycReDG9\nU142W6M//56H5cdxy7ujOmA96ixE1ve+fqronWDCf7W7F1C+6nOKp2t7Vd8t11K/\nq+IzeVEUWQUdWYTAWrXmScaRuHmVDi5BSgjUmKM4xHRPJ3c7acmQd3dIn7qS9qMh\npQlEOkvsAirPaZmyVLFc33JEahkXi5tcbxw7lmDJo8D67NO8EH0hPbSEK/lYmGG6\nDlrU8gLvAgMBAAECggEAIw/zhX2FYoRSalmZA48vlce5ZV27z8OOhiQ9r3L7ukji\nS2anqGhbV+0gTt3Z0/BGumctPK/Byp2Mbo2fLFShaAepitZqnGNpXgcIYKoExaK1\npwpPMTjyLsv2BzJ3f06+f0doq8f77IEji3gkBQZRrmPA+tL72rC2TT1yml3QO9Zb\nigoMbUPwM5QuC8vQD1edDeedPBy8T0f7zdx2j++83Bh5uHRQOyFxNBYzfiAC3670\nYikNYnJAHIepCatWpw+yCP6aLT75wxN30aF96/z4OUy9hORjce+Bw2tjay6poiCR\nl5QNAsTBasPZaBiIz/fEAgu3oYBscpJhZ4s5ubn3EQKBgQDv1RKZDmhe+IhGDVw1\n2QGp6qolRmmIWij6UTDrF0iu+HNeLHCZfE+5Qb6px2TjclVQky6mDIrac7ohDPkY\nkz/g3qeUJF0F+mPM0ij4MdDnO4ehGSwyesqFMJU0JWra42t+zMXOEgJ5MCsifXgx\nfX3/ZK0RMZBkizSKUdFP31UjLQKBgQDbWeL4XT5cI7O5fT6oLErynbr+PAVgHxZC\nUc+eCyZngRp+2voHKdBe6hAix/T3Qjpl9w9apIfjQ1QZcZvXWDXJ6MnodN3bg7ZB\nqGujmuKLDa5CbbbHdKUOkqU0N8Q0KHHlJ4xDJ5GE+7+tjjcerzJ5e41wsv1uXQ2r\nULBmMV+ACwKBgGN0P18bG2rus0whdDCcSdVVi7MjbNXvVXjgPGHw6OIuA2F1Gkh7\nxW+0dMVg+1RacEiWkEypfNc0EGZuQ8nOHjo4+tMy9SRqfgJM0FKEDfYluIu0raBN\nTThIOkdCkPouPsB5WDmpPD9XGzwVPceAG8TR0fcET7VyCJqnbR4rJdoNAoGBAJUZ\nFTljEBdLQUftBSEE8nDVnBxhqfm7R8MOnwQ7agBi9iKSL++ckYFislMh/bXwM4fJ\nloszWRa2VjzxR0/qKq2y6UNz5LXoYoNgusG7bw+73d5TezE6bVNphJfo7BnUAA8W\ngbXH+JcfFBwhlf/qcHG49NxNHgzdfYQcVbsxBkwfAoGBALfgyDQ4006UUNX6YQ6T\n8uuuFPiGYAeQnqcMlseWAUAU4M7UPTtP5e7idqBPPha5P+0j41fLR0Uq++ePUxID\nvA1qkACZDltEjQlg4H+5q2w/FUjdafFEpdLfAmx8sXpHB6H4049Kxpe5DrnM9zh3\nkpu9hE/OwDvrZeBscVlnrvct";
        string PUBLICKEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAzX98ny65QjPabG0NWctq\nepqnDyLHq16atddWHrF0d7QY0tRAQ8QyAcAtVG+9Aezr9eDylL3krFSYHBG2sUlB\nK9ftAm0CH+onUaYgr4OU2TSvf8sHXHLp+FtR5S38pv5wUXRYiQMnEXgxvVNeNluj\nP/+eh+XHccu7ozpgPeosRNb3vn6q6J1gwn+1uxdQvupziqdre1XfLddSv6viM3lR\nFFkFHVmEwFq15knGkbh5lQ4uQUoI1JijOMR0Tyd3O2nJkHd3SJ+6kvajIaUJRDpL\n7AIqz2mZslSxXN9yRGoZF4ubXG8cO5ZgyaPA+uzTvBB9IT20hCv5WJhhug5a1PIC\n7wIDAQAB";

        TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"),6789);
        tcpListener.Start();
        Console.WriteLine("server started");
        
        
        
        // var rsa = new RSACryptoServiceProvider();
        // string publicKeyXML = rsa.ToXmlString(false);
        // string privateKeyXML = rsa.ToXmlString(true);
        // RSAParameters funnyparams = rsa.ExportParameters(true);
        // var x = new XmlSerializer(funnyparams.GetType());
        // x.Serialize(Console.Out, funnyparams);
        
        string path = @"C:\Users\haane\RiderProjects\PizaServer\PizzaServer\keys.xml";
        string xmlString =  System.IO.File.ReadAllText(path);
        RSAParameters params2;
        using (StringReader reader = new StringReader(xmlString))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(RSAParameters));
            params2 = (RSAParameters)serializer.Deserialize(reader);
        }

        var rsa = new RSACryptoServiceProvider();
        rsa.ImportParameters(params2);
                    
        byte[] dataToSend = Encoding.UTF8.GetBytes("Hello world!");
        byte[]encryptedData = rsa.Encrypt(dataToSend, false);
                    
        string messageEncryptedReceived = Encoding.UTF8.GetString(encryptedData);
                    
        byte[] decryptedData = rsa.Decrypt(encryptedData, false);
        string messageReceived = Encoding.UTF8.GetString(decryptedData);
                    
        Console.WriteLine(messageEncryptedReceived);
        Console.WriteLine(messageReceived);
        
        
        
        
        TcpClient tcpClient = tcpListener.AcceptTcpClient();
        Console.WriteLine("client connected");
        NetworkStream stream = tcpClient.GetStream();

        //enter to an infinite cycle to be able to handle every change in stream
        while (true) {
            while(tcpClient.Available < 3){}

            byte[] bytes = new byte[tcpClient.Available];

            stream.Read(bytes, 0, bytes.Length);

            //translate bytes of request to string
            String data = Encoding.UTF8.GetString(bytes);
            // Console.WriteLine(data);
            
            if (Regex.IsMatch(data, "^GET")) {
                Console.WriteLine("We are getting a GET request");
                const string eol = "\r\n";
                string requestType = new Regex("Type: (.*)").Match(data).Groups[1].Value.Trim();
                string message = new Regex("Message: (.*)").Match(data).Groups[1].Value.Trim();
                
                Console.WriteLine("Request type = " + requestType);

                byte[] response;
                if (requestType == "request-public-key")
                {
                    response = Encoding.UTF8.GetBytes("PIZZA/1.1 200 OK" + eol
                        + "public-key: " + PUBLICKEY     
                    + eol);    
                }
                else if (requestType == "pizza")
                {
                    response = Encoding.UTF8.GetBytes("PIZZA/1.1 400 OK" + eol
                         + "AYO WTF" 
                         + eol);
                    
                    byte[] decrypyed_message =
                        RSA.Create().Decrypt(Encoding.UTF8.GetBytes(message), RSAEncryptionPadding.OaepSHA256);
                    
                }
                else
                {
                    response = Encoding.UTF8.GetBytes("PIZZA/1.1 400 OK" + eol
                        + "AYO WTF"     
                        + eol);
                }
                
                
                
                String responseStr = Encoding.UTF8.GetString(response);
                Console.WriteLine("res: " + responseStr);

                stream.Write(response, 0, response.Length);
            } else {
                Console.WriteLine("We dont have a GET request");
            }
        }
    }
}