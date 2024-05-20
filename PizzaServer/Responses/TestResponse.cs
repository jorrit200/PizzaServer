namespace PizzaServer.Responses;

public class TestResponse: IResponse
{
    public void Send(byte[] message)
    {
        Console.WriteLine(message);
    }
}