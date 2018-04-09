/*******************************************************************************
* Copyright (C) 2012 eBay Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Configuration;
using AsimovDeploy.WinAgent.Framework.Deployment.Steps;
using AsimovDeploy.WinAgent.Framework.Models.UnitActions;
using AsimovDeploy.WinAgent.Framework.Tasks;
using AsimovDeploy.WinAgent.Framework.WebSiteManagement;

namespace AsimovDeploy.WinAgent.Framework.Models.Units
{
    public interface IWebSiteDeployUnit : IInstallable
    {
        string SiteName { get; set; }
        string SiteUrl { get; set; }
    }

    public class WebSiteDeployUnit : DeployUnit, ICanBeStopStarted, IWebSiteDeployUnit, ICanUninistall
    {
        private string siteName;
        private int _lastUnitStatus;
        
        public bool CleanDeploy { get; set; }

        public AsimovTask GetStopTask() => new StartStopWebApplicationTask(this, true);

        public override void SetupDeployActions()
        {
            CleanDeploy = true; // default to true
            Actions.Add(new StartDeployUnitAction { Sort = 10 });
            Actions.Add(new StopDeployUnitAction { Sort = 11 });

            Actions.Add(new UnInstallUnitAction { Sort = 20 });
            _lastUnitStatus = (int)UnitStatus.NA;
        }

        public AsimovTask GetStartTask() => new StartStopWebApplicationTask(this, false);

        public AsimovTask GetUninstallTask()
        {
            var webServer = GetWebServer();
            var siteData = webServer.GetInfo();
            return new PowershellUninstallTask(Installable, this, new Dictionary<string, object>
            {
                {"SiteName", SiteName},
                {"SiteUrl", SiteUrl}
            })
            {
                TargetPath = siteData.PhysicalPath
            };
        }

        public string SiteName
        {
            get { return siteName ?? Name; }
            set { siteName = value; }
        }

        public string SiteUrl { get; set; }

        public InstallableConfig Installable { get; set; }

        public override string UnitType => DeployUnitTypes.WebSite;

        public override AsimovTask GetDeployTask(AsimovVersion version, ParameterValues parameterValues, AsimovUser user,
            string correlationId)
        {
            var task = new DeployTask(this, version, parameterValues, user, correlationId);
            if (CanInstall())
                task.AddDeployStep(new InstallWebSite(this));
            else
                task.AddDeployStep<UpdateWebSite>();
            foreach (var action in Actions.OfType<VerifyCommandUnitAction>())
                task.AddDeployStep(new ExecuteUnitAction(action, user));
            return task;
        }

        public override ActionParameterList GetDeployParameters()
        {
            if (CanInstall())
                return Installable.InstallParameters ?? new ActionParameterList();
            return DeployParameters;
        }

        private bool CanInstall()
        {
            return
                (UnitStatus)_lastUnitStatus == UnitStatus.NotFound &&
                Installable?.Install != null;
        }

        public virtual IWebServer GetWebServer() => new IIS7WebServer(SiteName, SiteUrl);

        public override DeployUnitInfo GetUnitInfo(bool refreshUnitStatus)
        {
            var siteInfo = base.GetUnitInfo(refreshUnitStatus);

            if ((UnitStatus)_lastUnitStatus == UnitStatus.NotFound)
            {
                siteInfo.Version = new DeployedVersion { VersionNumber = "0.0.0.0" };
            }

            siteInfo.Url = SiteUrl.Replace("localhost", HostNameUtil.GetFullHostName());
            siteInfo.Status = (UnitStatus)_lastUnitStatus;

            return siteInfo;
        }

        protected override void UpdateUnitStatus()
        {
            var siteData = GetWebServer().GetInfo();

            var status = siteData == null
                ? UnitStatus.NotFound
                : (siteData.AppPoolStarted && siteData.SiteStarted ? UnitStatus.Running : UnitStatus.Stopped);

            Interlocked.Exchange(ref _lastUnitStatus, (int) status);
        }
    }
}