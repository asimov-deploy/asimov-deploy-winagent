using System;
using System.Collections.Generic;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Configuration;
using AsimovDeploy.WinAgent.Web.Contracts;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.IntegrationTests.Scenarios.GetDeployUnitsScenario
{
    [TestFixture]
    public class GetDeployUnitsScenario : WinAgentSystemTest
    {
        public override void Given()
        {
            GivenFoldersForScenario();
            GivenRunningAgent();
        }

        [Test]
        public void should_return_all_deploy_units_when_no_querystrings_specified()
        {
            var units = Agent.Get<List<DeployUnitInfoDTO>>("/units/list");
            units.Count.ShouldBe(3);
        }

        [Test]
        public void should_return_deploy_units_when_agent_group_interesting_group()
        {
            var url = GetUrl(agentGroups: new[]
            {
                "Interesting Group"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(2);
        }

        [Test]
        public void should_return_deploy_units_when_agent_group_other_group()
        {
            var url = GetUrl(agentGroups: new[]
            {
                "Other Group"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(1);
        }

        [Test]
        public void should_return_deploy_units_when_multiple_existing_agent_groups()
        {
            var url = GetUrl(agentGroups: new[]
            {
                "Other Group",
                "Interesting Group"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(3);
        }

        [Test]
        public void should_not_return_any_deploy_units_when_non_existing_agent_group()
        {
            var url = GetUrl(agentGroups: new[]
            {
                "Non Existing Group"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(0);
        }

        [Test]
        public void should_return_deploy_units_when_existing_and_non_existing_agent_group()
        {
            var url = GetUrl(agentGroups: new[]
            {
                "Interesting Group",
                "Non Existing Group"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(2);
        }

        [Test]
        public void should_return_deploy_units_when_unit_group_test_1()
        {
            var url = GetUrl(unitGroups: new[]
            {
                "Test 1"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(2);
            units.Select(x => x.group).Distinct().First().ShouldBe("Test 1");
        }

        [Test]
        public void should_return_deploy_units_when_unit_group_test_2()
        {
            var url = GetUrl(unitGroups: new[]
            {
                "Test 2"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(1);
            units.Select(x => x.group).Distinct().First().ShouldBe("Test 2");
        }

        [Test]
        public void should_return_deploy_units_when_multiple_existing_unit_groups()
        {
            var url = GetUrl(unitGroups: new[]
            {
                "Test 1",
                "Test 2"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(3);
            units.Select(x => x.group).ShouldContain("Test 1");
            units.Select(x => x.group).ShouldContain("Test 2");
        }

        [Test]
        public void should_not_return_any_deploy_units_when_non_existing_unit_group()
        {
            var url = GetUrl(unitGroups: new[]
            {
                "Non Existing Group"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(0);
        }

        [Test]
        public void should_return_deploy_units_when_existing_and_non_existing_unit_group()
        {
            var url = GetUrl(unitGroups: new[]
            {
                "Test 1",
                "Non Existing Group"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(2);
            units.Select(x => x.group).Distinct().First().ShouldBe("Test 1");
        }

        [Test]
        public void should_return_deploy_units_when_unit_type_windows_service()
        {
            var url = GetUrl(unitTypes: new[]
            {
                DeployUnitTypes.WindowsService
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(3);
            units.Select(x => x.type).Distinct().First().ShouldBe(DeployUnitTypes.WindowsService);
        }

        [Test]
        public void should_not_return_any_deploy_units_when_non_existing_unit_type()
        {
            var url = GetUrl(unitTypes: new[]
            {
                DeployUnitTypes.WebSite
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(0);
        }

        [Test]
        public void should_return_deploy_units_when_unit_type_existing_and_non_existing()
        {
            var url = GetUrl(unitTypes: new[]
            {
                DeployUnitTypes.WindowsService,
                DeployUnitTypes.WebSite
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(3);
            units.Select(x => x.type).Distinct().First().ShouldBe(DeployUnitTypes.WindowsService);
        }

        [Test]
        public void should_return_deploy_units_when_tag_tag_1()
        {
            var url = GetUrl(tags: new[]
            {
                "tag1"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(3);
        }

        [Test]
        public void should_return_deploy_units_when_tag_tag_2()
        {
            var url = GetUrl(tags: new[]
            {
                "tag2"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(1);
        }

        [Test]
        public void should_return_deploy_units_when_tag_tag_3()
        {
            var url = GetUrl(tags: new[]
            {
                "tag3"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(1);
        }

        [Test]
        public void should_return_deploy_units_when_multiple_existing_tags()
        {
            var url = GetUrl(tags: new[]
            {
                "tag1",
                "tag2"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(3);
        }

        [Test]
        public void should_not_return_any_deploy_units_when_non_existing_tag()
        {
            var url = GetUrl(tags: new[]
            {
                "Non Existing Tag"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(0);
        }

        [Test]
        public void should_return_deploy_units_when_existing_and_non_existing_tag()
        {
            var url = GetUrl(tags: new[]
            {
                "tag3",
                "Non Existing Tag"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(1);
        }

        [Test, TestCaseSource(nameof(CombinedFilterCases))]
        public void should_return_deploy_units_when_combining_filters(string[] agentGroups, string[] unitGroups, string[] unitTypes, string[] tags, int expectedUnits)
        {
            var url = GetUrl(agentGroups, unitGroups, unitTypes, tags);
            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(expectedUnits);
        }

        private static readonly object[] CombinedFilterCases =
        {
            new object[]
            {
                new[] { "Interesting Group", "Other Group" },
                new[] { "Test 1", "Test 2" },
                new[] { DeployUnitTypes.WindowsService },
                new[] { "tag1", "tag2, tag3" },
                3
            },
            new object[]
            {
                new[] { "Interesting Group" },
                new[] { "Test 1", "Test 2" },
                new[] { DeployUnitTypes.WindowsService },
                new[] { "tag1" },
                2
            },
            new object[]
            {
                new[] { "Interesting Group" },
                new[] { "Test 1", "Test 2" },
                new[] { DeployUnitTypes.WindowsService },
                new[] { "tag2" },
                1
            }
        };

        private static string GetUrl(string[] agentGroups = null, string[] unitGroups = null, string[] unitTypes = null, string[] tags = null)
        {
            var url = "/units/list";
            var querystrings = new Dictionary<string, string[]>();

            if (agentGroups != null)
            {
                querystrings.Add("agentGroups", agentGroups);
            }

            if (unitGroups != null)
            {
                querystrings.Add("unitGroups", unitGroups);
            }

            if (unitTypes != null)
            {
                querystrings.Add("unitTypes", unitTypes);
            }

            if (tags != null)
            {
                querystrings.Add("tags", tags);
            }

            if (querystrings.Any())
            {
                url += "?";
            }

            return url + string.Join("&", querystrings.Select(x => $"{x.Key}=" + string.Join($"&{x.Key}=", x.Value)));
        }
    }
}