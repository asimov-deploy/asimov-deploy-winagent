using System.Collections.Generic;
using AsimovDeploy.WinAgent.Web.Contracts;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.IntegrationTests.Scenarios.GroupScenario
{
    [TestFixture]
    public class GroupScenario : WinAgentSystemTest
    {
        public override void Given()
        {
            GivenFoldersForScenario();
            GivenRunningAgent();
        }

        [Test]
        public void can_get_deploy_units_by_group_name()
        {
            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list/Interesting Group");
            units.Count.ShouldBe(2);
        }

        [Test]
        public void can_get_all_deploy_units_when_no_group_name_specified()
        {
            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
            units.Count.ShouldBe(3);
        }
    }
}