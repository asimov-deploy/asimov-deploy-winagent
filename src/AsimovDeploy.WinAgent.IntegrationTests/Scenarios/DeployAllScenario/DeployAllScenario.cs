using System;
using System.Collections.Generic;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Configuration;
using AsimovDeploy.WinAgent.Web.Commands;
using AsimovDeploy.WinAgent.Web.Contracts;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.IntegrationTests.Scenarios.DeployAllScenario
{
    [TestFixture]
    public class DeployAllScenario : WinAgentSystemTest
    {
        public override void Given()
        {
            GivenFoldersForScenario();
            GivenRunningAgent();
            EnsureServicesIsNotInstalled();
        }

        [TearDown]
        public void AfterEach()
        {
            EnsureServicesIsNotInstalled();
        }

        private void EnsureServicesIsNotInstalled()
        {
            foreach (var unit in GetInstallableUnits())
            {
                if (unit.status == "NotFound")
                {
                    return;
                }

                Agent.Post("/action", NodeFront.ApiKey, new UnitActionCommand()
                {
                    actionName = "Uninstall",
                    unitName = unit.name
                });
                WaitForStatus(unit, "NotFound");
            }
        }

        [Test]
        public void can_get_deploy_units()
        {
            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
            units.Count.ShouldBe(5);
        }

        [Test]
        public void installs_service_on_deploy()
        {
            InstallService();

            foreach (var unit in GetInstallableUnits())
            {
                WaitForStatus(unit, "Running");
            }

            foreach (var unit in GetInstallableUnits())
            {
                Agent.Post("/action", NodeFront.ApiKey, new UnitActionCommand()
                {
                    actionName = "Stop",
                    unitName = unit.name
                });
            }

            foreach (var unit in GetInstallableUnits())
            {
                WaitForStatus(unit, "Stopped");
            }
        }

        private void InstallService(string preferredBranch = null)
        {
            Agent.Post("/deploy/deploy-all-units", NodeFront.ApiKey, new DeployAllCommand
            {
                preferredBranch = preferredBranch
            });
        }

        private IEnumerable<DeployUnitInfoDTO> GetInstallableUnits()
        {
            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
            return units.Where(x => x.type == DeployUnitTypes.WindowsService || x.type == DeployUnitTypes.WebSite);
        }

        private void WaitForStatus(DeployUnitInfoDTO unit, string expectedStatus)
        {
            var start = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(60);

            TimeSpan duration;
            string status;

            do
            {
                duration = DateTime.Now - start;
                var units = Agent.Get<List<DeployUnitInfoDTO>>($"/units/list?units={unit.name}");
                units.Count.ShouldBe(1);
                status = units[0].status;
                if (status == expectedStatus)
                {
                    return;
                }
            } while (duration < timeout);
            
            Assert.Fail($"Failed to go to correct status for {unit.name}, was {status} expected {expectedStatus}");
        }
    }
}