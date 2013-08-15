using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Interact.Examples.Test
{
    [TestClass]
    public class MoneyTransferTest
    {

        [TestMethod]
        public void ExecuteMoneyTransfer()
        {
            //Silly test that just shows nothing was reported
            var res = Interact.Execute(@"..\..\..\Examples\MoneyTransfer\MoneyTransfer.csproj");
            Assert.AreEqual("", res);
        }
    }
}
