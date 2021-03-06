using System;
using System.Collections.Generic;
using System.IO;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Events;
using AsimovDeploy.WinAgent.Framework.Models.Units;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
    public class PowershellUninstallTask : AsimovTask
    {
        private readonly InstallableConfig _installableConfig;
        private readonly DeployUnit _unit;
        private readonly Dictionary<string, object> _unitParameters;
        private readonly NodeFront _nodefront = new NodeFront();

        public PowershellUninstallTask(InstallableConfig config, DeployUnit unit, Dictionary<string, object> unitParameters)
        {
            _installableConfig = config;
            _unit = unit;
            _unitParameters = unitParameters;
        }

        public string TargetPath { get; set; }

        protected override void Execute()
        {
            Log.Info($"Uninstalling {_unit.Name}...");

            if (string.IsNullOrEmpty(_installableConfig.TargetPath))
                _installableConfig.TargetPath = Path.Combine(@"\Services", _unit.Name);
            if (!string.IsNullOrEmpty(_installableConfig.Uninstall))
                UnInstallUsingCommandFromConfig();
            else if (_installableConfig.InstallType.Equals("NServiceBus", StringComparison.InvariantCultureIgnoreCase))
                UnInstallNServiceBusHandler();
            else if (_installableConfig.InstallType.Equals("TopShelf", StringComparison.InvariantCultureIgnoreCase))
                UnInstallTopShelfService();
            else
            {
                Log.Warn($"Uninstall of {_unit.Name} not supported. Please check InstallableConfig.");
                return;
            }

            _nodefront.Notify(new UnitStatusChangedEvent(_unit.Name, _unit.GetUnitInfo(true).Status));
            Log.Info($"Uninstall of {_unit.Name} done!");
        }

        private void UnInstallNServiceBusHandler()
        {
            ;
            var exePath = $"{GetTargetPath()}\\NServiceBus.Host.exe";
            var serviceName = (_unit as WindowsServiceDeployUnit)?.ServiceName ?? _unit.Name;
            var args = new List<string>
            {
                "/uninstall",
                "/servicename:" + serviceName,
                "-f",
                TargetPath ?? _installableConfig.TargetPath
            };
            ProcessUtil.ExecuteCommand(exePath, args.ToArray(), Log);

        }

        private void UnInstallTopShelfService()
        {
            var exePath = $"{GetTargetPath()}\\{_installableConfig.AssemblyName ?? _unit.Name}.exe";
            ProcessUtil.ExecuteCommand(exePath, new[] { "uninstall" }, Log);

        }

        private void UnInstallUsingCommandFromConfig()
        {
            ProcessUtil.ExecutePowershellScript(
                GetTargetPath(),
                _installableConfig.Uninstall,
                _unitParameters, Log,
                new[] { _installableConfig.ScriptsDir });

        }

        private string GetTargetPath()
        {
            if (!string.IsNullOrEmpty(TargetPath) && Directory.Exists(TargetPath))
            {
                return TargetPath;
            }
            return _installableConfig.TargetPath;
        }
    }
}