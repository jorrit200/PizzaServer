using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PizzaServer;

namespace PizzaServer.Tests;

[TestClass]
[TestSubject(typeof(Validator))]
public class ValidatorTest
{

    [TestMethod]
    public void ValidateTimeStamp()
    {
        Assert.IsTrue(Validator.TimeStamp().Validate("05/12/2022 18:32"));
        Assert.IsFalse(Validator.TimeStamp().Validate("05/122/2022 18:32"));
        Assert.IsFalse(Validator.TimeStamp().Validate("05/12/2022 18:32 ik ben sjone"));
    }

    [TestMethod]
    public void ValidateAddress()
    {
        Assert.IsTrue(Validator.Address().Validate("De bananenlaan 12"));
        Assert.IsTrue(Validator.Address().Validate("Hoppa 2"));
        Assert.IsFalse(Validator.Address().Validate("hoppa2"));
        Assert.IsFalse(Validator.Address().Validate("H0ppa 2"));
        Assert.IsFalse(Validator.Address().Validate("Hoppa 2 e"));
    }

    [TestMethod]
    public void ValidateArea()
    {
        Assert.IsTrue(Validator.Area().Validate("9251 LA Burgum"));
        Assert.IsFalse(Validator.Area().Validate("9251 LA Bur2gum"));
        Assert.IsFalse(Validator.Area().Validate("925 LA Burgum"));
        Assert.IsFalse(Validator.Area().Validate("9251 LA burgum"));
        Assert.IsFalse(Validator.Area().Validate("925 La Burgum"));
    }
}