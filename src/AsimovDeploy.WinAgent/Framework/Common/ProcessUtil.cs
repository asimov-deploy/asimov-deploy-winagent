/*******************************************************************************
* Copyright (C) 2012 eBay Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
******************************************************************************/

using System;
using System.Diagnostics;
using AsimovDeploy.WinAgent.Framework.Deployment;
using log4net;

namespace AsimovDeploy.WinAgent.Framework.Common
{
    public class ProcessUtil
    {
        public static void ExecutePowershellScript(string workingDirectory, string command, ILog log)
        {
            using (var p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.WorkingDirectory = workingDirectory;
                p.StartInfo.FileName = @"C:\Windows\system32\WindowsPowerShell\v1.0\powershell.exe";
                p.StartInfo.CreateNoWindow = true;

                p.StartInfo.Arguments = "-NoProfile -ExecutionPolicy Unrestricted -NonInteractive -";

                p.Start();

                p.StandardOutput.RedirectOutput(log.Info);
                p.StandardError.RedirectOutput(log.Error);
                p.StandardInput.WriteLine(command);
                p.StandardInput.Close();

                p.WaitForExit((int) TimeSpan.FromMinutes(40).TotalMilliseconds);

                if (p.ExitCode != 0)
                    throw new DeployException("Powershell script did not complete successfully");
            }
        }
    }
}