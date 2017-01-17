using System.Collections.Generic;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Events;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.Units;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
    public class PowershellUninstallTask : AsimovTask
    {
        private readonly InstallableConfig _config;
        private readonly DeployUnit _unit;
        private readonly Dictionary<string, object> unitParameters;
        private readonly NodeFront _nodefront = new NodeFront();
        public PowershellUninstallTask(InstallableConfig config, DeployUnit unit, Dictionary<string,object> unitParameters)
        {
            _config = config;
            _unit = unit;
            this.unitParameters = unitParameters;
        }

        protected override void Execute()
        {
            Log.Info(_config.TargetPath);
            ProcessUtil.ExecutePowershellScript(
                _config.TargetPath, //TODO: We may want to use the location of the service in the future
                _config.Uninstall, 
                unitParameters, Log);

            _nodefront.Notify(new UnitStatusChangedEvent(_unit.Name, _unit.GetUnitInfo().Status));
        }
    }
}