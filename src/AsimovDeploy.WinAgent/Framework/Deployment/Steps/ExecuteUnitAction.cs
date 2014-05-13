using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.UnitActions;

namespace AsimovDeploy.WinAgent.Framework.Deployment.Steps
{
    public class ExecuteUnitAction : IDeployStep
    {
        private readonly UnitAction action;
        private readonly AsimovUser user;

        public ExecuteUnitAction(UnitAction action, AsimovUser user)
        {
            this.action = action;
            this.user = user;
        }

        public void Execute(DeployContext context)
        {
            action.GetTask(context.DeployUnit, user).ExecuteTask();
        }
    }
}
