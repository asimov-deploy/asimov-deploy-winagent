using System;
using System.ServiceProcess;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Events;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using AsimovDeploy.WinAgent.Framework.Tasks.ServiceControl;
using log4net;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
    public class StartStopWindowsServiceTask : AsimovTask
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(StartStopWindowsServiceTask));

        private readonly WindowsServiceDeployUnit _unit;
        private readonly bool _stop;
        private readonly NodeFront _nodefront = new NodeFront();

        public StartStopWindowsServiceTask(WindowsServiceDeployUnit unit, bool stop)
        {
            _unit = unit;
            _stop = stop;
        }

        protected override string InfoString() => (_stop ? "Stopping " : "Starting ") + _unit.Name;

        protected override void Execute()
        {
            var intermediateStatus = _stop ? UnitStatus.Stopping : UnitStatus.Starting;
            var endStatus = _stop ? UnitStatus.Stopped : UnitStatus.Running;

            _nodefront.Notify(new UnitStatusChangedEvent(_unit.Name, intermediateStatus));

            using (var controller = new ServiceController(_unit.ServiceName))
            {
                if (_stop) StopService(controller);
                else StartService(controller);
            }

            var unitInfo = _unit.GetUnitInfo(true);

            if (unitInfo.Status != endStatus)
            {
                _log.Error($"Failed to {(_stop ? "stop" : "start")} {_unit.Name}");
            }

            _nodefront.Notify(new UnitStatusChangedEvent(_unit.Name, unitInfo.Status));
        }

        private void StartService(ServiceController controller)
        {
            controller.Start();
            controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(2));
        }

        private static void StopService(ServiceController controller)
        {
            if (controller.Status == ServiceControllerStatus.Running)
                controller.StopServiceAndWaitForExit();
        }
    }


}