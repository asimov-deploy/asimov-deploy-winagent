using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AsimovDeploy.WinAgent.Web.Commands;
using AsimovDeploy.WinAgent.Web.Contracts;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.IntegrationTests.Scenarios.GoogleStorageScenario
{
    [TestFixture]
    public class GoogleStorageScenario : WinAgentSystemTest
    {
        public override void Given()
        {
            GivenFoldersForScenario();
            GivenRunningAgent();
        }

        [Test]
        public void can_get_deploy_units()
        {
            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
            units.Count.ShouldBe(3);
        }


        [Test]
        public void can_get_versions_for_unit1()
        {
            var versions = Agent.Get<List<DeployUnitVersionDTO>>($"/versions/Unit1");
            versions.Count.ShouldBe(1);
        }

        [Test]
        public void can_get_versions_for_unit2()
        {
            var versions = Agent.Get<List<DeployUnitVersionDTO>>($"/versions/Unit2");
            versions.Count.ShouldBe(2);
        }

        [Test]
        public void can_get_versions_for_unitmulti()
        {
            var versions = Agent.Get<List<DeployUnitVersionDTO>>($"/versions/UnitMulti");
            versions.Count.ShouldBe(100);
            versions.First().version.ShouldBe("1.0.0.201");
        }

        [Test]
        public void gets_versions_sorted_by_timestamp_descending()
        {
            var versions = Agent.Get<List<DeployUnitVersionDTO>>($"/versions/UnitMulti");
            var timestamp = DateTime.MaxValue;
            foreach (var v in versions)
            {
                var currentTimestamp = DateTime.Parse(v.timestamp);
                timestamp.ShouldBeGreaterThanOrEqualTo(timestamp);
                timestamp = currentTimestamp;
            }
            DateTime.Parse(versions.First().timestamp).ShouldBeGreaterThanOrEqualTo(DateTime.Parse(versions.Last().timestamp));
        }

        [Test]
        public void can_deploy_unit1()
        {
            var versions = Agent.Get<List<DeployUnitVersionDTO>>($"/versions/Unit1");
            versions.Count.ShouldBe(1);

            Agent.Post("/deploy/deploy", NodeFront.ApiKey, new DeployCommand
            {
                unitName = "Unit1",
                versionId = versions[0].id,
            });

            Thread.Sleep(1000);

            File.Exists(Path.Combine(DataDir, "Unit1Target\\somefile.txt")).ShouldBe(true);

        }
    }
}