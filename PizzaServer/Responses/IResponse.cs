namespace PizzaServer.Responses;

public interface IResponse
{
    public void Send(byte[] message);
}