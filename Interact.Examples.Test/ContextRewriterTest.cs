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
        [TestMethod] //Not really a test just executing stuff
        public void TransformAccount()
        {
            var solution = Interact.Transformation.WorkspaceExtensions.GetSolution(@"..\..\..\Examples\MoneyTransfer\Account.cs");
            solution = solution.Rewrite();
            solution.Projects.First().WriteToFile();
        }

    }
}
