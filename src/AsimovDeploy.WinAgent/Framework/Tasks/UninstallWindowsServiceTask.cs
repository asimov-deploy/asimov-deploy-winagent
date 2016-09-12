using System.Collections.Generic;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Events;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.Units;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
    public class UninstallWindowsServiceTask : AsimovTask
    {
        private readonly InstallableConfig _config;
        private readonly DeployUnit _unit;
        private readonly NodeFront _nodefront = new NodeFront();
        public UninstallWindowsServiceTask(InstallableConfig windowsConfigInstallConfig, DeployUnit unit)
        {
            _config = windowsConfigInstallConfig;
            _unit = unit;
        }

        protected override void Execute()
        {
            ProcessUtil.ExecutePowershellScript(
                _config.TargetPath, //TODO: We may want to use the location of the service in the future
                _config.Uninstall, 
                new ParameterValues(new Dictionary<string,object>()), Log);

            _nodefront.Notify(new UnitStatusChangedEvent(_unit.Name, _unit.GetUnitInfo().Status));
        }
    }
}