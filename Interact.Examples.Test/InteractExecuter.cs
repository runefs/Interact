using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interact.Examples.Test
{
    public static class InteractExecuter
    {

        public static string Execute(string solutionOrProjectPath, string arguments=null)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "Interact.exe";
            startInfo.Arguments = solutionOrProjectPath + " " + string.Join(" ", arguments);
            startInfo.RedirectStandardOutput = true;
            process.StartInfo = startInfo;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }
    }
}
