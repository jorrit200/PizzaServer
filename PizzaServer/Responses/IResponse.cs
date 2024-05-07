namespace PizzaServer;

public interface IResponse
{
    public void Send(byte[] message);
}