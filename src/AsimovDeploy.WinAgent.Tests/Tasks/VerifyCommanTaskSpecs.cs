using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using AsimovDeploy.WinAgent.Framework.Tasks;
using AsimovDeploy.WinAgent.Web.Commands;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.Tests.Tasks
{
    [TestFixture]
    public class VerifyCommanTaskSpecs
    {
        private FakeNotifier _fakeNotifier;
        private TestVerifyCommandTask _verifyTask;
        private FakeNotifier _notifier;
        private AsimovConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = new AsimovConfig();
            _verifyTask = new TestVerifyCommandTask(
                _config,
                new WebSiteDeployUnit(),
                "zippath",
                "command",
                "3A477B55-52BE-45D2-9650-A5E6DD607B24"
            );
        }


        [Test]
        public void relative_report_paths_should_be_prepended_with_agent_web_control_url()
        {
            _verifyTask.ParseVerifyCommandOutput("##asimov-deploy[report='relative-path' title='my report']");
            Assert.AreEqual($"{_config.WebControlUrl}temp-reports/relative-path", _verifyTask.Report.url);
        }

        [Test]
        public void absolute_report_paths_should_be_passed_on_without_modification()
        {
            _verifyTask.ParseVerifyCommandOutput("##asimov-deploy[report='http://test.local/report.html' title='my report']");
            Assert.AreEqual("http://test.local/report.html", _verifyTask.Report.url);
        }
    }

    class TestVerifyCommandTask : VerifyCommandTask
    {
        public TestVerifyCommandTask(
            IAsimovConfig config,
            WebSiteDeployUnit webSiteDeployUnit, 
            string zipPath, 
            string command, 
            string correlationId) : base(webSiteDeployUnit, zipPath, command, correlationId)
        {
            _config = config;
        }

        protected override IAsimovConfig Config => _config;

        public Report Report => base.report;
    }
}
