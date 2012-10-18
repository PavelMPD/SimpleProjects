using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Finance;

namespace FinanceTest
{
    [TestClass]
    public class CurrencyTest
    {
        [TestMethod]
        public void TestMultiplication()
        {
            Money five = Money.NewDollar(5);
            Assert.AreEqual(Money.NewDollar(10), five.Times(2));
            Assert.AreEqual(Money.NewDollar(15), five.Times(3));
        }

        [TestMethod]
        public void TestEquality()
        {
            Assert.IsTrue(Money.NewDollar(5).Equals(Money.NewDollar(5)));
            Assert.IsFalse(Money.NewDollar(5).Equals(Money.NewDollar(6)));
            Assert.IsTrue(Money.NewFranc(5).Equals(Money.NewFranc(5)));
            Assert.IsFalse(Money.NewFranc(5).Equals(Money.NewFranc(6)));
            Assert.IsFalse(Money.NewDollar(5).Equals(Money.NewFranc(5)));
        }

        [TestMethod]
        public void TestFrancMultiplication()
        {
            Money five = Money.NewFranc(5);
            Assert.AreEqual(Money.NewFranc(10), five.Times(2));
            Assert.AreEqual(Money.NewFranc(15), five.Times(3));
        }

        [TestMethod]
        public void TestCurrency()
        {
            Assert.AreEqual("USD", Money.NewDollar(1).Currency());
            Assert.AreEqual("CHF", Money.NewFranc(1).Currency());
        }
    }
}
