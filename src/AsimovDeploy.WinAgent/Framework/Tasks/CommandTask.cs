using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models.Units;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
    public class CommandTask : AsimovTask
    {
        private readonly DeployUnit deployUnit;
        private readonly string command;
        private readonly string correlationId;

        public CommandTask(DeployUnit deployUnit, string command, string correlationId)
        {
            this.deployUnit = deployUnit;
            this.command = command;
            this.correlationId = correlationId;
        }

        protected override void Execute()
        {
            var cmd = new LaunchCommandAsProcess(Config.TempFolder);
            cmd.OutputReceived += launch_OutputReceived;

            cmd.SendCommand(command);
            cmd.SyncClose();
        }

        protected void launch_OutputReceived(object sendingProcess, EventArgsForCommand e)
        {
            var data = e.OutputData;
            Log.InfoFormat("Executing {0} - {1}", GetTaskName(), data);
            Log.Debug(data);
        }

    }
}