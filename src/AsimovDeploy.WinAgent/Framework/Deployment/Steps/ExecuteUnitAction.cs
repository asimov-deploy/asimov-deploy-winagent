using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.UnitActions;

namespace AsimovDeploy.WinAgent.Framework.Deployment.Steps
{
    public class ExecuteUnitAction : IDeployStep
    {
        public UnitAction Action { get; }
        public AsimovUser User { get; }

        public ExecuteUnitAction(UnitAction action, AsimovUser user)
        {
            Action = action;
            User = user;
        }

        public void Execute(DeployContext context)
        {
            Action.GetTask(context.DeployUnit, User, context.CorrelationId).ExecuteTask();
        }
    }
}
