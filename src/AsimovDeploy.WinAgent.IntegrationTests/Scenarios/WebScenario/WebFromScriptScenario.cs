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
    public class WebFromScriptScenario : WebScenarioBase
    {
        protected override string ServiceName => "Asimov.Web.Example.From.Script";

        [Test]
        public void can_get_deploy_units()
        {
            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
            units[1].name.ShouldBe(ServiceName);
            units[1].status.ShouldBe("NotFound");
            units[1].type.ShouldBe(DeployUnitTypes.WebSite);
        }

        [Test]
        public void installs_service_on_deploy()
        {
            InstallService();

            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
            units[1].name.ShouldBe(ServiceName);
            units[1].status.ShouldBe("Running");
            units[1].type.ShouldBe(DeployUnitTypes.WebSite);
        }
    }
}