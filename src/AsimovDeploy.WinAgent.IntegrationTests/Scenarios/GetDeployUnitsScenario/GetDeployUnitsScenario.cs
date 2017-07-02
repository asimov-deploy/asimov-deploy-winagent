using System.Collections.Generic;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Configuration;
using AsimovDeploy.WinAgent.Framework.Models;
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
            var url = GenerateUrl(agentGroups: new[]
            {
                "Interesting Group"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(2);
        }

        [Test]
        public void should_return_deploy_units_when_agent_group_other_group()
        {
            var url = GenerateUrl(agentGroups: new[]
            {
                "Other Group"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(1);
        }

        [Test]
        public void should_return_deploy_units_when_multiple_existing_agent_groups()
        {
            var url = GenerateUrl(agentGroups: new[]
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
            var url = GenerateUrl(agentGroups: new[]
            {
                "Non Existing Group"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(0);
        }

        [Test]
        public void should_return_deploy_units_when_existing_and_non_existing_agent_group()
        {
            var url = GenerateUrl(agentGroups: new[]
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
            var url = GenerateUrl(unitGroups: new[]
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
            var url = GenerateUrl(unitGroups: new[]
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
            var url = GenerateUrl(unitGroups: new[]
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
            var url = GenerateUrl(unitGroups: new[]
            {
                "Non Existing Group"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(0);
        }

        [Test]
        public void should_return_deploy_units_when_existing_and_non_existing_unit_group()
        {
            var url = GenerateUrl(unitGroups: new[]
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
            var url = GenerateUrl(unitTypes: new[]
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
            var url = GenerateUrl(unitTypes: new[]
            {
                DeployUnitTypes.WebSite
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(0);
        }

        [Test]
        public void should_return_deploy_units_when_unit_type_existing_and_non_existing()
        {
            var url = GenerateUrl(unitTypes: new[]
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
            var url = GenerateUrl(tags: new[]
            {
                "tag1"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(3);
        }

        [Test]
        public void should_return_deploy_units_when_tag_tag_2()
        {
            var url = GenerateUrl(tags: new[]
            {
                "tag2"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(1);
        }

        [Test]
        public void should_return_deploy_units_when_tag_tag_3()
        {
            var url = GenerateUrl(tags: new[]
            {
                "tag3"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(1);
        }

        [Test]
        public void should_return_deploy_units_when_multiple_existing_tags()
        {
            var url = GenerateUrl(tags: new[]
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
            var url = GenerateUrl(tags: new[]
            {
                "Non Existing Tag"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(0);
        }

        [Test]
        public void should_return_deploy_units_when_existing_and_non_existing_tag()
        {
            var url = GenerateUrl(tags: new[]
            {
                "tag3",
                "Non Existing Tag"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(1);
        }

        [Test]
        public void should_return_deploy_units_when_unit_is_empty_string()
        {
            var url = GenerateUrl(units: new[]
            {
                string.Empty
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldNotBe(0);
        }

        [Test]
        public void should_return_deploy_units_when_unit_is_null()
        {
            var url = GenerateUrl(units: new string[]
            {
                null
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldNotBe(0);
        }

        [Test]
        public void should_return_deploy_units_when_unit_TestService1()
        {
            var url = GenerateUrl(units: new[]
            {
                "TestService1"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Single().name.ShouldBe("TestService1");
        }

        [Test]
        public void should_return_deploy_units_when_unit_TestService2()
        {
            var url = GenerateUrl(units: new[]
            {
                "TestService2"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Single().name.ShouldBe("TestService2");
        }

        [Test]
        public void should_return_deploy_units_when_unit_UnitWithParameters()
        {
            var url = GenerateUrl(units: new[]
            {
                "UnitWithParameters"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Single().name.ShouldBe("UnitWithParameters");
        }

        [Test]
        public void should_return_deploy_units_when_unit_starts_with_test()
        {
            var url = GenerateUrl(units: new[]
            {
                "test.*"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(2);
        }

        [Test]
        public void should_return_deploy_units_when_unit_has_service_in_the_middle()
        {
            var url = GenerateUrl(units: new[]
            {
                ".*service.*"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(2);
        }

        [Test]
        public void should_return_deploy_units_when_unit_contains_e()
        {
            var url = GenerateUrl(units: new[]
            {
                ".*e.*"
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(3);
        }

        [Test]
        public void should_return_deploy_units_when_unit_status_not_found()
        {
            var url = GenerateUrl(unitStatuses: new[]
            {
                UnitStatus.NotFound.ToString()
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(3);
            units.Select(x => x.status).Distinct().First().ShouldBe(UnitStatus.NotFound.ToString());
        }

        [Test]
        public void should_not_return_any_deploy_units_when_non_existing_unit_status()
        {
            var url = GenerateUrl(unitStatuses: new[]
            {
                UnitStatus.Running.ToString()
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(0);
        }

        [Test]
        public void should_return_deploy_units_when_unit_status_existing_and_non_existing()
        {
            var url = GenerateUrl(unitStatuses: new[]
            {
                UnitStatus.NotFound.ToString(),
                UnitStatus.Running.ToString()
            });

            var units = Agent.Get<List<DeployUnitInfoDTO>>(url);
            units.Count.ShouldBe(3);
            units.Select(x => x.status).Distinct().First().ShouldBe(UnitStatus.NotFound.ToString());
        }

        [Test, TestCaseSource(nameof(CombinedFilterCases))]
        public void should_return_deploy_units_when_combining_filters(string[] agentGroups, string[] unitGroups, string[] unitTypes, string[] tags, string[] unitNames, string[] unitStatuses, int expectedUnits)
        {
            var url = GenerateUrl(agentGroups, unitGroups, unitTypes, tags, unitNames, unitStatuses);
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
                new[] { "TestService1", "TestService2", "UnitWithParameters" },
                new[] { UnitStatus.NotFound.ToString() },
                3
            },
            new object[]
            {
                new[] { "Interesting Group" },
                new[] { "Test 1", "Test 2" },
                new[] { DeployUnitTypes.WindowsService },
                new[] { "tag1" },
                new string[] {},
                new string[] {},
                2
            },
            new object[]
            {
                new[] { "Interesting Group" },
                new[] { "Test 1", "Test 2" },
                new[] { DeployUnitTypes.WindowsService },
                new[] { "tag1" },
                new[] { ".*service.*" },
                new string[] {},
                2
            },
            new object[]
            {
                new[] { "Interesting Group" },
                new[] { "Test 1", "Test 2" },
                new[] { DeployUnitTypes.WindowsService },
                new[] { "tag2" },
                new string[] {},
                new string[] {},
                1
            },
            new object[]
            {
                new[] { "Interesting Group" },
                new[] { "Test 1", "Test 2" },
                new[] { DeployUnitTypes.WindowsService },
                new[] { "tag2" },
                new [] { ".*Service2" },
                new string[] {},
                1
            }
        };

        private static string GenerateUrl(string[] agentGroups = null, string[] unitGroups = null, string[] unitTypes = null, string[] tags = null, string[] units = null, string[] unitStatuses = null)
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

            if (units != null)
            {
                querystrings.Add("units", units);
            }

            if (unitStatuses != null)
            {
                querystrings.Add("unitStatus", unitStatuses);
            }

            if (querystrings.Any())
            {
                url += "?";
            }

            return url + string.Join("&", querystrings.Select(x => $"{x.Key}=" + string.Join($"&{x.Key}=", x.Value)));
        }
    }
}