using System;
using System.Collections.Generic;
using System.IO;
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

            var webSite = (WebSiteDeployUnit) config.Units[0];
            webSite.Name.ShouldBe("DefaultSite");
            webSite.SiteName.ShouldBe("DeployTestWeb");
            webSite.SiteUrl.ShouldBe("http://localhost/DefaultSite");
            webSite.PackageInfo.InternalPath.ShouldBe("DefaultSitePath");
            webSite.PackageInfo.Source.ShouldBe("Prod");
            
           
            webSite.CleanDeploy.ShouldBe(true);
        }

        [Test]
        public void can_read_agent_config_with_specific_env()
        {
            var config = ReadConfig("ConfigExamples", "testAgent1");

            config.NodeFrontUrl.ShouldBe("http://overriden:3335");
        }

        [Test]
        public void can_read_package_source()
        {
            var config = ReadConfig("ConfigExamples", "asd");

            config.PackageSources.Count.ShouldBe(3);

            var source1 = (FileSystemPackageSource) config.PackageSources[0];
            source1.Uri.ShouldBe(new Uri("file://test"));

            var source2 = (FileSystemPackageSource)config.PackageSources[1];
            source2.Uri.ShouldBe(new Uri("file://test2"));

            var source3 = (AsimovWebPackageSource)config.PackageSources[2];
            source3.Uri.ShouldBe(new Uri("http://asimov"));
        }
        
        [Test]
        public void can_read_unit_actions()
        {
            var config = ReadConfig("ConfigExamples", "asd");

            config.Units[0].Actions.Count.ShouldBe(4);
            
            config.Units[0].Actions[2].ShouldBeTypeOf<VerifyUrlsUnitAction>();
            config.Units[0].Actions[3].ShouldBeTypeOf<VerifyCommandUnitAction>();
            
            var commandAction = (VerifyCommandUnitAction)config.Units[0].Actions[3];
            commandAction.ZipPath.ShouldBe("SiteVerify.zip");
            commandAction.Command.ShouldBe("phantomjs.exe");
        }
	
        [Test]
        public void env_config_file_can_override_and_add_packages_sources_and_units()
        {
            var config = ReadConfig("ConfigExamples", "testAgent1");
            
            config.PackageSources.Count.ShouldBe(4);
            config.Units.Count.ShouldBe(2);

            var packageSource = config.GetPackageSourceFor(config.Units[1]);
            ((FileSystemPackageSource)packageSource).Uri.ShouldBe(new Uri("file://extra"));
        }

        [Test]
        public void can_have_deploy_unit_with_deploy_parameters()
        {
            var config = ReadConfig("ConfigExamples", "deploy-parameters");
            var unit = config.GetUnitByName("UnitWithParameters");

            unit.HasDeployParameters.ShouldBe(true);
            unit.DeployParameters[0].ShouldBeTypeOf<TextActionParameter>();
            ((TextActionParameter)unit.DeployParameters[0]).Default.ShouldBe("Deploy-Everything");
        }

        [Test]
        public void can_read_one_custom_parameter_for_load_balancer_and_return_a_querystring()
        {
            var config = ReadConfig("ConfigExamples", "testAgent1");

            var queryString = config.GetLoadBalancerParametersAsQueryString();

            queryString.ShouldBe("partition=testgroup1");
        }

        [Test]
        public void can_read_custom_parameters_for_load_balancer_and_return_a_querystring()
        {
            var config = ReadConfig("LoadbalancerConfig", "testAgent1");

            var queryString = config.GetLoadBalancerParametersAsQueryString();

            queryString.ShouldBe("partition=testgroup1&host=a+host");
        }

        public AsimovConfig ReadConfig(string configDir, string agentName)
        {
            return (AsimovConfig)new ConfigurationReader().Read(configDir, agentName);
        }
    }
}