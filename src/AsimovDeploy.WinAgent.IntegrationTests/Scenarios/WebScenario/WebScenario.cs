using System.Collections.Generic;
using AsimovDeploy.WinAgent.Framework.Configuration;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Web.Contracts;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.IntegrationTests.Scenarios.WebScenario
{
    [TestFixture]
    public class WebScenario : WebScenarioBase
    {
        protected override string ServiceName => "Asimov.Web.Example";

        [Test]
        public void can_get_deploy_units()
        {
            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
            units[0].name.ShouldBe(ServiceName);
            units[0].status.ShouldBe("NotFound");
            units[0].type.ShouldBe(DeployUnitTypes.WebSite);
        }

        [Test]
        public void installs_service_on_deploy()
        {
            InstallService();

            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
            units[0].name.ShouldBe(ServiceName);
            units[0].status.ShouldBe("Running");
            units[0].type.ShouldBe(DeployUnitTypes.WebSite);
        }

        [Test]
        public void when_NotFound_gets_install_parameters()
        {
            var parameters = Agent.Get<List<TextActionParameter>>($"/units/deploy-parameters/{ServiceName}");

            parameters.Count.ShouldBe(1);
            parameters[0].Name.ShouldBe("Port");
            parameters[0].Default.ShouldBe("8123");
        }

        [Test]
        public void when_Installed_gets_deploy_parameters()
        {
            InstallService();

            var parameters = Agent.Get<List<TextActionParameter>>($"/units/deploy-parameters/{ServiceName}");

            parameters.Count.ShouldBe(1);
            parameters[0].Name.ShouldBe("NotUsed");
        }
    }
}