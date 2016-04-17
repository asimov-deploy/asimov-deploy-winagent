using System;
using System.Collections.Generic;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Web.Commands;
using AsimovDeploy.WinAgent.Web.Contracts;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.IntegrationTests.Scenarios.ServiceScenario
{
    [TestFixture]
    public class WindowsServiceScenario : WinAgentSystemTest
    {
        private const string ServiceName = "Asimov.Roundhouse.Example";

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
            if (units[0].status == "NotFound")
            {
                return;
            }
            Agent.Post("/action",NodeFront.ApiKey, new UnitActionCommand()
            {
                actionName = "Uninstall",
                unitName = ServiceName
            });
            WaitForStatus("NotFound");
        }

        [Test]
        public void can_get_deploy_units()
        {
            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
            units.Count.ShouldBe(1);
            units[0].name.ShouldBe(ServiceName);
            units[0].status.ShouldBe("NotFound");
        }

        [Test]
        public void installs_service_on_deploy()
        {
            var versions = Agent.Get<List<DeployUnitVersionDTO>>($"/versions/{ServiceName}");
            versions.Count.ShouldBe(1);


            Agent.Post("/deploy/deploy", NodeFront.ApiKey, new DeployCommand
            {
                unitName = ServiceName,
                versionId = versions[0].id,
            });

            WaitForStatus("Stopped");

            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
            units.Count.ShouldBe(1);
            units[0].name.ShouldBe(ServiceName);
            units[0].status.ShouldBe("Stopped");

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
                units.Count.ShouldBe(1);
                status = units[0].status;
                if (status == expectedStatus)
                {
                    return;
                }
            } while (duration < timeout);
            
            Assert.Fail($"Failed to go to correct status, was {status} expected {expectedStatus}");

        }
    }
}