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
using System.ServiceProcess;
using System.Threading;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Configuration;
using AsimovDeploy.WinAgent.Framework.Deployment.Steps;
using AsimovDeploy.WinAgent.Framework.Models.UnitActions;
using AsimovDeploy.WinAgent.Framework.Tasks;

namespace AsimovDeploy.WinAgent.Framework.Models.Units
{
    public class WindowsServiceDeployUnit : DeployUnit, ICanBeStopStarted, ICanUninistall, IInstallableService, ICanBeKilled
    {
        private string _serviceName;
        private int _lastUnitStatus;
        public string ServiceName
        {
            get => _serviceName ?? Name;
            set => _serviceName = value;
        }

        public string Url { get; set; }
        public InstallableConfig Installable { get; set; }
        public bool CanBeKilled { get; set; }

        public override void SetupDeployActions()
        {
            Actions.AddIfFirstOfType(new StartDeployUnitAction { Sort = 10 });
            Actions.AddIfFirstOfType(new StopDeployUnitAction { Sort = 11 });
            
            if (CanBeKilled)
                Actions.AddIfFirstOfType(new KillDeployUnitAction { Sort = 12 });

            if (Installable?.IsUninstallable == true)
                Actions.AddIfFirstOfType(new UnInstallUnitAction() { Sort = 20 });

            _lastUnitStatus = (int)UnitStatus.NA;
        }


        public override string UnitType => DeployUnitTypes.WindowsService;

        public override AsimovTask GetDeployTask(AsimovVersion version, ParameterValues parameterValues, AsimovUser user, string correlationId)
        {
            var task = new DeployTask(this, version, parameterValues, user, correlationId);
            if (CanInstall)
                task.AddDeployStep(new InstallWindowsService(this));
            else
                task.AddDeployStep<UpdateWindowsService>();

            foreach (var action in Actions.OfType<CommandUnitAction>())
            {
                task.AddDeployStep(new ExecuteUnitAction(action, user));
            }
            return task;
        }

        private bool CanInstall => (UnitStatus)_lastUnitStatus == UnitStatus.NotFound &&
                                     (Installable?.Install != null || Installable?.InstallType != null);


        public override ActionParameterList GetDeployParameters() => CanInstall ? Installable.GetInstallAndCredentialParameters() : DeployParameters;

        public override DeployUnitInfo GetUnitInfo(bool refreshUnitStatus)
        {
            var unitInfo = base.GetUnitInfo(refreshUnitStatus);

            if (!string.IsNullOrEmpty(Url))
            {
                unitInfo.Url = Url.Replace("localhost", HostNameUtil.GetFullHostName());
            }

            unitInfo.Status = (UnitStatus)_lastUnitStatus;

            return unitInfo;
        }

        protected override void UpdateUnitStatus()
        {
            var serviceManager = new ServiceController(ServiceName);
            UnitStatus status;

            try
            {
                status = serviceManager.Status == ServiceControllerStatus.Running
                    ? UnitStatus.Running
                    : UnitStatus.Stopped;
            }
            catch
            {
                status = UnitStatus.NotFound;
            }

            Interlocked.Exchange(ref _lastUnitStatus, (int)status);
        }

        public AsimovTask GetStopTask() => new StartStopWindowsServiceTask(this, stop: true);
        public AsimovTask GetStartTask() => new StartStopWindowsServiceTask(this, stop: false);
        public AsimovTask GetKillTask() => new KillWindowsServiceTask(this);

        public AsimovTask GetUninstallTask()
        {
            return new PowershellUninstallTask(
                Installable,
                this,
                new Dictionary<string, object>() { { "ServiceName", ServiceName } })
            {
                TargetPath = WindowsServiceUtil.GetWindowsServicePath(ServiceName)
            };
        }
    }
}