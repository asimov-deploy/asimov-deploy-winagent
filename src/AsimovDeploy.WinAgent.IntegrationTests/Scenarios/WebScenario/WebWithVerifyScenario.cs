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
    [TestFixture]
    public class WebWithVerifyScenario : WinAgentSystemTest
    {
        private const string ServiceName = "Asimov.Web.Example.With.Verify.Folder";

        public override void Given()
        {
            GivenFoldersForScenario();
            GivenRunningAgent();
            EnsureServiceIsNotInstalled();
        }

        [TearDown]
        public void AfterEach()
        {
            EnsureServiceIsNotInstalled();
        }

        private void EnsureServiceIsNotInstalled()
        {
            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
            if (units[1].status == "NotFound")
            {
                return;
            }
            Agent.Post("/action",NodeFront.ApiKey, new UnitActionCommand()
            {
                actionName = "Uninstall",
                unitName = ServiceName
            });
            WaitForStatus("NotFound");
            while (Process.GetProcesses().Any(x => x.ProcessName == "Asimov.Roundhouse.Example"))
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
        }

        [Test]
        public void can_run_verify_after_deploy()
        {
            InstallService();

            Agent.Post("/action", NodeFront.ApiKey, new UnitActionCommand() {actionName = "Verify", correlationId = Guid.NewGuid().ToString(), unitName = ServiceName});

            WaitForAgentOutput("verify-success");

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

        private void InstallService()
        {
            var versions = Agent.Get<List<DeployUnitVersionDTO>>($"/versions/{ServiceName}");
            versions.Count.ShouldBe(1);

            Agent.Post("/deploy/deploy", NodeFront.ApiKey, new DeployCommand
            {
                unitName = ServiceName,
                versionId = versions[0].id,
                parameters = new Dictionary<string, object>() {
                {
                    "Port", "8145"
                } }
            });

            WaitForStatus("Running");
        }

        private void WaitForStatus(string expectedStatus)
        {
            var start = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(10);
            var duration = DateTime.Now - start;
            var status = "";

            do
            {
                duration = DateTime.Now - start;
                var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
                units.Count.ShouldBe(3);
                status = units[2].status;
                if (status == expectedStatus)
                {
                    return;
                }
            } while (duration < timeout);
            
            Assert.Fail($"Failed to go to correct status, was {status} expected {expectedStatus}");

        }
    }
}