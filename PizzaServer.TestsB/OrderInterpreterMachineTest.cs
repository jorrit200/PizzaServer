using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PizzaServer;

namespace PizzaServer.TestsB;

[TestClass]
[TestSubject(typeof(OrderInterpreterMachine))]
public class OrderInterpreterMachineTest
{

    [TestMethod]
    public void InterpretOrder()
    {
        OrderInterpreterMachine orderInterpreter = new OrderInterpreterMachine(
            "Jorrit\nBurgermeester Bothenius 4\n1234 AA Burgum\nTonno\n2\n3\nTonijn\nUi\nChampignons\n02/07/2024 01:26".Split("\n").ToList().GetEnumerator(),
            new Dictionary<string, int>(){ { "Tonno", 1 }},
            new Dictionary<string, int>(){{"Tonijn", 1}, {"Ui", 1}, {"Champignons", 1}}
            );
        orderInterpreter.Interpret();
    }
}