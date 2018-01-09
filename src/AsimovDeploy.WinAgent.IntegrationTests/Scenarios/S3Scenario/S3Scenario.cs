using System.Collections.Generic;
using System.IO;
using System.Threading;
using AsimovDeploy.WinAgent.Web.Commands;
using AsimovDeploy.WinAgent.Web.Contracts;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.IntegrationTests.Scenarios.S3Scenario
{
    [TestFixture, Ignore(@"This test needs the following dependencies setup to work:
1) An S3 bucket with the files in the s3Bucket folder
2) The aws cli configured so that the default profile has access to that bucket
")]
    public class S3Scenario : WinAgentSystemTest
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
            units.Count.ShouldBe(2);
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