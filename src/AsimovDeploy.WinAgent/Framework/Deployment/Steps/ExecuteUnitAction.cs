using AsimovDeploy.WinAgent.Framework.Models.UnitActions;

namespace AsimovDeploy.WinAgent.Framework.Deployment.Steps
{
    public class ExecuteUnitAction : IDeployStep
    {
        private readonly UnitAction action;

        public ExecuteUnitAction(UnitAction action)
        {
            this.action = action;
        }

        public void Execute(DeployContext context)
        {
            action.GetTask(context.DeployUnit).ExecuteTask();
        }
    }
}