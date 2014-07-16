using System.Diagnostics;
using System.IO;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
    public class LaunchCommandAsProcess
    {
        public delegate void OutputEventHandler(object sendingProcess, EventArgsForCommand e);

        public event OutputEventHandler OutputReceived;
        private StreamWriter stdIn;
        private Process p;

        public void SendCommand(string command)
        {
            stdIn.WriteLine(command);
        }

        public LaunchCommandAsProcess(string workingdirectory)
        {
            p = new Process
            {
                StartInfo =
                {
                    FileName = @"C:\Windows\System32\cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            if (!string.IsNullOrEmpty(workingdirectory))
                p.StartInfo.WorkingDirectory = workingdirectory;

            p.Start();

            stdIn = p.StandardInput;
            p.OutputDataReceived += Process_OutputDataReceived;
            p.ErrorDataReceived += Process_OutputDataReceived;
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

        }

        private void Process_OutputDataReceived(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data == null)
                return;
            else
            {
                if (OutputReceived != null)
                {
                    var e = new EventArgsForCommand();
                    e.OutputData = outLine.Data;
                    OutputReceived(this, e);
                }
            }
        }


        public void SyncClose()
        {
            stdIn.WriteLine("exit");
            p.WaitForExit();
            p.Close();
        }

        public void AsyncClose()
        {
            stdIn.WriteLine("exit");
            p.Close();
        }
    }
}