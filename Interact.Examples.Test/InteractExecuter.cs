using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interact.Examples.Test
{
    public static class Interact
    {

        public static string Execute(string solutionOrProjectPath)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "Interact.exe";
            startInfo.Arguments = solutionOrProjectPath;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd();
        }
    }
}
