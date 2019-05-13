using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsimovDeploy.WinAgent.Framework.Deployment.Steps;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.UnitActions;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using AsimovDeploy.WinAgent.Framework.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using StructureMap;

namespace AsimovDeploy.WinAgent.Tests.DeployUnit
{
    [TestFixture]
    public class WebSiteDeployUnitTests
    {

        private static DeployTask DeployTask(ParameterValues parameterValues, WebSiteDeployUnit webSiteDeployUnit)
        {
            var task = (DeployTask) webSiteDeployUnit.GetDeployTask(new AsimovVersion(), parameterValues,
                new AsimovUser(), "1");
            return task;
        }

        private static WebSiteDeployUnit UnitWithVerify()
        {
            ObjectFactory.Initialize(o => { o.For<IAsimovConfig>().Use(new AsimovConfig()); });
            var unit = new WebSiteDeployUnit();

            unit.Actions.Add(new VerifyCommandUnitAction());
            return unit;
        }
        [Test]
        public void auto_runs_verify_test()
        {
            var deployUnit = UnitWithVerify();

            var task = DeployTask(new ParameterValues(new Dictionary<string, dynamic>()), deployUnit);

            var steps = task.Select(x=>x()).ToArray();
            Assert.IsInstanceOf<ExecuteUnitAction>(steps.Last());

            var action = (ExecuteUnitAction) steps.Last();
            Assert.IsInstanceOf<VerifyCommandUnitAction>(action.Action);
        }

        [Test]
        public void skips_verify_test_when_skip_verify_is_set()
        {
            var deployUnit = UnitWithVerify();

            var task = DeployTask(new ParameterValues(new Dictionary<string, dynamic> {{"SkipVerify", true}}), deployUnit);

            var steps = task.Select(x=>x()).ToArray();
            Assert.IsNotInstanceOf<ExecuteUnitAction>(steps.Last());

        }

        
    }
}
