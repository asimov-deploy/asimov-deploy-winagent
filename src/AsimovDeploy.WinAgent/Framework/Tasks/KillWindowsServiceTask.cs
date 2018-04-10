using System;
using System.ServiceProcess;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Events;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using AsimovDeploy.WinAgent.Framework.Tasks.ServiceControl;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
    public class KillWindowsServiceTask : AsimovTask
    {
        private readonly WindowsServiceDeployUnit _unit;

        public KillWindowsServiceTask(WindowsServiceDeployUnit unit) => _unit = unit;
        protected override string InfoString() => "Killing " + _unit.Name;

        protected override void Execute()
        {
            var nodeFront = new NodeFront();
            try
            {
                using (var controller = new ServiceController(_unit.ServiceName))
                    controller.KillServiceProcess();
                nodeFront.Notify(new UnitStatusChangedEvent(_unit.Name, UnitStatus.Stopped));
            }
            catch (Exception e)
            {
                Log.Warn($"Could not kill {_unit.ServiceName}. Reason: ({e.Message})");
                var unitInfo = _unit.GetUnitInfo(true);
                nodeFront.Notify(new UnitStatusChangedEvent(_unit.Name, unitInfo.Status));
            }
        }
    }
}