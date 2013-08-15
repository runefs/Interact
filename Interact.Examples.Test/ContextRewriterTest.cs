using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interact.Transformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using System.IO;

namespace Interact.Examples.Test
{
    [TestClass]
    public class ContextRewriterTest
    {
        [TestMethod]
        public void TransformAccount()
        {
            var transformer = new Transformer(@"..\..\..\Examples\MoneyTransfer\Account.cs");
            var solution = transformer.GetSolution();
            solution = transformer.RewriteSolution(solution);
            transformer.WriteProjectToFile(solution.Projects.First());
        }

    }
}
