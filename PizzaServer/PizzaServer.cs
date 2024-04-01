// See https://aka.ms/new-console-template for more information
using System.Text;
using System.Text.RegularExpressions;   
using System.Net;

namespace PizzaServer;

using System.Net.Sockets;

internal class PizzaServer
{
    public static void Main(string[] args)
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"),6789);
        tcpListener.Start();
        Console.WriteLine("server started");
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
                const string eol = "\r\n"; // HTTP/1.1 defines the sequence CR LF as the end-of-line marker

                byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
                    + "Connection: Upgrade" + eol
                    + "Upgrade: websocket" + eol
                    + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                        System.Security.Cryptography.SHA1.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(
                                new System.Text.RegularExpressions.Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                            )
                        )
                    ) + eol
                    + eol);
                
                String responseStr = Encoding.UTF8.GetString(response);
                Console.WriteLine("res: " + responseStr);

                stream.Write(response, 0, response.Length);
            } else {
                Console.WriteLine("We dont have a GET request");
            }
        }
    }
}