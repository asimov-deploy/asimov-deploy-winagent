using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using AsimovDeploy.WinAgent.Framework.Configuration;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Web.Commands;
using AsimovDeploy.WinAgent.Web.Contracts;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.IntegrationTests.Scenarios.WebScenario
{
    public class WebWithVerifyScenario : WebScenarioBase
    {
        protected override string ServiceName => "Asimov.Web.Example.With.Verify";

        [Test]
        public void runs_verify_folder_after_deploy()
        {
            InstallService();

            WaitForAgentOutput("verify-output-folder");
        }
        
        [Test]
        public void runs_verify_zip_after_deploy()
        {
            InstallService();

            WaitForAgentOutput("verify-output-zip");
        }

        private void WaitForAgentOutput(string output)
        {
            var start = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(10);
            var duration = DateTime.Now - start;

            do
            {
                duration = DateTime.Now - start;
                if (AgentOutput.Length > output.Length && AgentOutput.ToString().Contains(output))
                {
                    return;
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            } while (duration < timeout);
            
            StringAssert.Contains(output,AgentOutput.ToString(), "Agent output didnt contain expected string");
        }
        
    }
}