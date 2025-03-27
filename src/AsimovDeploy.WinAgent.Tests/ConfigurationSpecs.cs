using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Configuration;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.PackageSources;
using AsimovDeploy.WinAgent.Framework.Models.UnitActions;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.Tests
{
    [TestFixture]
    public class ConfigurationSpecs
    {
        private TestEnvironmentVariableProvider _environmentVariableProvider;

        [SetUp]
        public void Setup()
        {
            _environmentVariableProvider = new TestEnvironmentVariableProvider();
        }

        public AsimovConfig ReadConfig(string configDir, string agentName)
        {
            return (AsimovConfig) new ConfigurationReader(_environmentVariableProvider).Read(Path.Combine(TestContext.CurrentContext.TestDirectory,configDir), agentName);
        }

        [Test]
        public void can_get_default_timeout_for_load_balancer()
        {
            var config = ReadConfig("LoadbalancerConfig", "testAgent1");

            var timeout = config.LoadBalancerTimeout;

            timeout.ShouldBe(30);
        }

        [Test]
        public void can_get_deploy_units_by_agent_group_using_multiple_environments()
        {
            var config = ReadConfig("ConfigExamples", "testagent3");
            var testUnits = config.GetUnitsByAgentGroup("Test Group");
            var otherUnits = config.GetUnitsByAgentGroup("Other Group");

            config.GetUnitsByAgentGroup().Count.ShouldBe(3);
            testUnits.Count.ShouldBe(1);
            otherUnits.Count.ShouldBe(1);

            var unit = config.GetUnitByName("UnitWithParameters");
            unit.ShouldNotBe(null);

            unit = config.GetUnitByName("TestService");
            unit.ShouldNotBe(null);
        }

        [Test]
        public void can_get_deploy_units_by_unit_group_using_multiple_environments()
        {
            var config = ReadConfig("ConfigExamples", "testagent3");
            var testUnits = config.GetUnitsByUnitGroup("Test");
            var defaultUnits = config.GetUnitsByUnitGroup("Default");

            testUnits.Count.ShouldBe(2);
            defaultUnits.Count.ShouldBe(1);

            var unit = config.GetUnitByName("UnitWithParameters");
            unit.ShouldNotBe(null);

            unit = config.GetUnitByName("TestService");
            unit.ShouldNotBe(null);
        }

        [Test]
        public void can_get_deploy_units_by_unit_type_using_multiple_environments()
        {
            var config = ReadConfig("ConfigExamples", "testagent3");
            var windowsServiceUnits = config.GetUnitsByType(DeployUnitTypes.WindowsService);
            var websiteUnits = config.GetUnitsByType(DeployUnitTypes.WebSite);

            windowsServiceUnits.Count.ShouldBe(2);
            windowsServiceUnits.Select(x => x.Name).ShouldContain("TestService");
            windowsServiceUnits.Select(x => x.Name).ShouldContain("UnitWithParameters");

            websiteUnits.Count.ShouldBe(1);
            websiteUnits.Select(x => x.Name).ShouldContain("DefaultSite");
        }

        [Test]
        public void can_get_deploy_units_by_tag_using_multiple_environments()
        {
            var config = ReadConfig("ConfigExamples", "testagent3");
            var tag1Units = config.GetUnitsByTag("tag1");
            var tag2Units = config.GetUnitsByTag("tag2");
            var tag3Units = config.GetUnitsByTag("tag3");
            var tag4Units = config.GetUnitsByTag("tag4");
            var tag5Units = config.GetUnitsByTag("tag5");

            tag1Units.Count.ShouldBe(3);
            tag1Units.Select(x => x.Name).ShouldContain("DefaultSite");
            tag1Units.Select(x => x.Name).ShouldContain("UnitWithParameters");
            tag1Units.Select(x => x.Name).ShouldContain("TestService");

            tag2Units.Count.ShouldBe(1);
            tag2Units.Select(x => x.Name).ShouldContain("DefaultSite");

            tag3Units.Count.ShouldBe(2);
            tag3Units.Select(x => x.Name).ShouldContain("TestService");
            tag3Units.Select(x => x.Name).ShouldContain("UnitWithParameters");

            tag4Units.Count.ShouldBe(2);
            tag4Units.Select(x => x.Name).ShouldContain("TestService");
            tag4Units.Select(x => x.Name).ShouldContain("UnitWithParameters");

            tag5Units.Count.ShouldBe(1);
            tag5Units.Select(x => x.Name).ShouldContain("UnitWithParameters");
        }

        [Test]
        public void can_have_deploy_unit_with_deploy_parameters()
        {
            var config = ReadConfig("ConfigExamples", "deploy-parameters");
            var unit = config.GetUnitByName("UnitWithParameters");

            unit.HasDeployParameters.ShouldBe(true);
            unit.DeployParameters[0].ShouldBeOfType<TextActionParameter>();
            ((TextActionParameter) unit.DeployParameters[0]).Default.ShouldBe("Deploy-Everything");

            unit.DeployParameters[1].ShouldBeOfType<PasswordActionParameter>();
            ((PasswordActionParameter)unit.DeployParameters[1]).Password.ShouldBe("Password!");
            ((PasswordActionParameter)unit.DeployParameters[1]).Default.ShouldBe(null);

            unit.DeployParameters[2].ShouldBeOfType<PasswordActionParameter>();
            ((PasswordActionParameter)unit.DeployParameters[2]).Password.ShouldBe(null);
            ((PasswordActionParameter)unit.DeployParameters[2]).Default.ShouldBe("DefaultPassword");
        }

        [Test]
        public void can_match_agent_names_with_regex_range()
        {
            var config1 = ReadConfig("LoadbalancerConfig", "testAgent1");
            var config2 = ReadConfig("LoadbalancerConfig", "testAgent2");
            var config5 = ReadConfig("LoadbalancerConfig", "testAgent5");
            var config6 = ReadConfig("LoadbalancerConfig", "testAgent6");

            var queryString1 = config1.GetLoadBalancerParametersAsQueryString();
            var queryString2 = config2.GetLoadBalancerParametersAsQueryString();
            var queryString5 = config5.GetLoadBalancerParametersAsQueryString();
            var queryString6 = config6.GetLoadBalancerParametersAsQueryString();

            queryString1.ShouldBe("partition=testgroup1&host=a+host");
            queryString2.ShouldBe("partition=testgroup1&host=a+host");
            queryString5.ShouldBe("partition=testgroup2&host=a+host");
            queryString6.ShouldBe("partition=testgroup2&host=a+host");
        }

        [Test]
        public void can_read_agent_config_with_specific_env()
        {
            var config = ReadConfig("ConfigExamples", "testAgent1");

            config.NodeFrontUrl.ShouldBe("http://overriden:3335");
        }

        [Test]
        public void Can_read_config_and_get_defaults()
        {
            var config = ReadConfig("ConfigExamples", "notmatching");

            config.Environment.ShouldBe("default");
            config.NodeFrontUrl.ShouldBe("http://default:3335");
            config.WebPort.ShouldBe(21233);
            config.HeartbeatIntervalSeconds.ShouldBe(10);
            config.TempFolder.ShouldBe("\\Data\\Temp");
            config.ConfigVersion.ShouldBe(101);
            config.Units[0].UnitType.ShouldBe(DeployUnitTypes.WebSite);

            var webSite = (WebSiteDeployUnit) config.Units[0];
            webSite.Name.ShouldBe("DefaultSite");
            webSite.Group.ShouldBe("Default");
            webSite.SiteName.ShouldBe("DeployTestWeb");
            webSite.SiteUrl.ShouldBe("http://localhost/DefaultSite");
            webSite.PackageInfo.InternalPath.ShouldBe("DefaultSitePath");
            webSite.PackageInfo.Source.ShouldBe("Prod");
            webSite.UnitType.ShouldBe(DeployUnitTypes.WebSite);
            webSite.Tags.ShouldContain("tag1");
            webSite.Tags.ShouldContain("tag2");

            webSite.CleanDeploy.ShouldBe(true);
        }

        [Test]
        public void can_read_custom_parameters_for_load_balancer_and_return_a_querystring()
        {
            var config = ReadConfig("LoadbalancerConfig", "testAgent1");

            var queryString = config.GetLoadBalancerParametersAsQueryString();

            queryString.ShouldBe("partition=testgroup1&host=a+host");
        }

        [Test]
        public void can_read_one_custom_parameter_for_load_balancer_and_return_a_querystring()
        {
            var config = ReadConfig("ConfigExamples", "testAgent1");

            var queryString = config.GetLoadBalancerParametersAsQueryString();

            queryString.ShouldBe("partition=testgroup1");
        }

        [Test]
        public void can_read_package_source()
        {
            var config = ReadConfig("ConfigExamples", "asd");

            config.PackageSources.Count.ShouldBe(3);

            var source1 = (FileSystemPackageSource) config.PackageSources[0];
            source1.Uri.ShouldBe(new Uri("file://test"));

            var source2 = (FileSystemPackageSource) config.PackageSources[1];
            source2.Uri.ShouldBe(new Uri("file://test2"));

            var source3 = (AsimovWebPackageSource) config.PackageSources[2];
            source3.Uri.ShouldBe(new Uri("http://asimov"));
        }

        [Test]
        public void can_read_unit_actions()
        {
            var config = ReadConfig("ConfigExamples", "asd");

            config.Units[0].Actions.OrderBy(x=>x.Sort).Select(x=>x.Name).ShouldBe(new []{"Rollback", "verify1", "verify2", "Start", "Stop"});

            config.Units[0].Actions[1].ShouldBeOfType<VerifyUrlsUnitAction>();
            config.Units[0].Actions[2].ShouldBeOfType<VerifyCommandUnitAction>();

            var commandAction = (VerifyCommandUnitAction) config.Units[0].Actions[2];
            commandAction.ZipPath.ShouldBe("SiteVerify.zip");
            commandAction.Command.ShouldBe("phantomjs.exe");
        }

        [Test]
        public void env_config_file_can_override_and_add_packages_sources_and_units()
        {
            var config = ReadConfig("ConfigExamples", "testAgent1");

            config.PackageSources.Count.ShouldBe(4);
            config.Units.Count.ShouldBe(2);

            var testService = (WindowsServiceDeployUnit)config.Units[1];
            var packageSource = config.GetPackageSourceFor(testService);
            ((FileSystemPackageSource)packageSource).Uri.ShouldBe(new Uri("file://extra"));

            testService.Tags.ShouldContain("tag3");
            testService.Tags.ShouldContain("tag4");
        }

        [Test]
        public void unit_without_package_source_should_have_null_package_source()
        {
            var config = ReadConfig("ConfigExamples", "no-packagesource-agent");

            config.Units.Count.ShouldBe(2);

            var testService = (WindowsServiceDeployUnit)config.Units[1];
            var packageSource = config.GetPackageSourceFor(testService);
            packageSource.ShouldBeOfType<NullPackageSource>();

        }

        [Test]
        public void can_get_agent_groups()
        {
            var config = ReadConfig("ConfigExamples", "testagent3");
            var agentGroups = config.GetAgentGroups();

            agentGroups.Length.ShouldBe(2);
            agentGroups.ShouldContain("Other Group");
            agentGroups.ShouldContain("Test Group");
        }

        [Test]
        public void can_handle_environment_substitutions()
        {
            var overridenUrl = "http://overriden-by-env:3333";
            _environmentVariableProvider.SetVariable("ASIMOV_DEPLOY_NODE_FRONT_URL", overridenUrl);
            var config = ReadConfig("ConfigExamples", "testagent4");
            
            config.NodeFrontUrl.ShouldBe(overridenUrl);
        }

        [Test]
        public void can_get_unit_groups()
        {
            var config = ReadConfig("ConfigExamples", "testagent3");
            var unitGroups = config.GetUnitGroups();

            unitGroups.Length.ShouldBe(2);
            unitGroups.ShouldContain("Default");
            unitGroups.ShouldContain("Test");
        }

        [Test]
        public void can_get_unit_types()
        {
            var config = ReadConfig("ConfigExamples", "testagent3");
            var unitTypes = config.GetUnitTypes();

            unitTypes.Length.ShouldBe(2);
            unitTypes.ShouldContain(DeployUnitTypes.WebSite);
            unitTypes.ShouldContain(DeployUnitTypes.WindowsService);
        }

        [Test]
        public void can_get_unit_tags()
        {
            var config = ReadConfig("ConfigExamples", "testagent3");
            var tags = config.GetUnitTags();

            tags.Length.ShouldBe(7);
            tags.ShouldContain("os:Windows");
            tags.ShouldContain($"host:{Environment.MachineName}");
            tags.ShouldContain("tag1");
            tags.ShouldContain("tag2");
            tags.ShouldContain("tag3");
            tags.ShouldContain("tag4");
            tags.ShouldContain("tag5");
        }

        [Test]
        public void can_get_unit_statuses()
        {
            var config = ReadConfig("ConfigExamples", "testagent3");
            var statuses = config.GetUnitStatuses();

            foreach (var unitStatus in Enum.GetValues(typeof(UnitStatus)))
            {
                statuses.ShouldContain(unitStatus.ToString());
            }

            foreach (var deployStatus in Enum.GetValues(typeof(DeployStatus)))
            {
                statuses.ShouldContain(deployStatus.ToString());
            }
        }
    }

    public class TestEnvironmentVariableProvider : IEnvironmentVariableProvider
    {
        private readonly Dictionary<string, string> _variables = new Dictionary<string, string>();

        public void SetVariable(string name, string value)
        {
            _variables[name] = value;
        }

        public string GetEnvironmentVariable(string name)
        {
            return _variables.TryGetValue(name, out var value) ? value : null;
        }
    }
}