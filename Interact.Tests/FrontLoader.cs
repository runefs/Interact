using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Interact.Test
{
    [TestClass]
    public class FrontLoaderTest
    {

        [TestMethod]
        public void ExecuteFrontLoaderTest()
        {
            //Silly test that just shows nothing was reported
            var res = InteractExecuter.Execute(@"..\..\..\Examples\FrontLoader\FrontLoader.csproj");
            Assert.AreEqual("", res);
        }
    }
}
