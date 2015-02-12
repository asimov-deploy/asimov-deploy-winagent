using System;
using System.Diagnostics;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Deployment;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
    public class PowershellCommandTask : AsimovTask
    {
        private readonly string _arguments;
        private readonly string _correlationId;

        public PowershellCommandTask(string arguments, string correlationId)
        {
            _arguments = arguments;
            _correlationId = correlationId;
        }

        protected override void Execute()
        {
            using (var p = new Process())
            {
                // RedirectOutput the output stream of the child process.
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.WorkingDirectory = ""; 
                p.StartInfo.FileName = @"C:\Windows\system32\WindowsPowerShell\v1.0\powershell.exe";
                p.StartInfo.CreateNoWindow = true;

                p.StartInfo.Arguments = _arguments;

                p.Start();

                p.StandardOutput.RedirectOutput(str => Log.Info(str));
                p.StandardError.RedirectOutput(str => Log.Info(str));

                p.WaitForExit((int)TimeSpan.FromMinutes(40).TotalMilliseconds);

                if (p.ExitCode != 0)
                    throw new DeployException("Powershell script did not complete successfully");
            }
        }
    }
}