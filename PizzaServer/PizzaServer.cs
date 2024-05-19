using PizzaServer.Observers;
using PizzaServer.Servers;

namespace PizzaServer;
internal static class PizzaServer
{
    public const string Eol = "\r\n";
    public const string Eot = "EOT;";
    private const string Protocol = "TCP";
    private const int Port = 6789;
    public static void Main()
    {
        IServerSubject subjectServer = Protocol == "TCP" ? new TcpSubjectServer(Port) : new UdpSubjectServer(Port);

        subjectServer.Attach(new KeyExchangeObserver(), "request-symmetric-key");
        subjectServer.Attach(new PizzaObserver(Menu, Toppings), "pizza");
        subjectServer.Attach(new MenuObserver(Menu), "menu");
        subjectServer.Start();
    }
    
    
    private static readonly Dictionary<string, int> Menu = new()
    {
        { "Margherita", 1000 },
        { "Tonno", 1250 },
        { "Calzone", 1300 }
    };

    private static readonly Dictionary<string, int> Toppings = new()
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
    
}