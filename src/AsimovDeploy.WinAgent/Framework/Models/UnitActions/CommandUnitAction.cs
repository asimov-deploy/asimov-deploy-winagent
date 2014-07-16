using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using AsimovDeploy.WinAgent.Framework.Tasks;

namespace AsimovDeploy.WinAgent.Framework.Models.UnitActions
{
    public class CommandUnitAction : UnitAction
    {
        public string Command { get; set; }

        public override AsimovTask GetTask(DeployUnit unit, AsimovUser user)
        {
            return new CommandTask(unit, Command);
        }
    }
}