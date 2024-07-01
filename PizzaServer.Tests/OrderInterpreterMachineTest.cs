using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PizzaServer;

namespace PizzaServer.Tests;

[TestClass]
[TestSubject(typeof(OrderInterpreterMachine))]
public class OrderInterpreterMachineTest
{

    [TestMethod]
    public void InterpretOrder()
    {
        var interpreter = new OrderInterpreterMachine();
        interpreter.Interpret("Jorrit\nBurgemeester Bothenius Lohmnalaan 13\n9251 LA Burgum");
    }
}