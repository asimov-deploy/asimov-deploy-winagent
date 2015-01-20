using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using AsimovDeploy.WinAgent.Framework.Tasks;

namespace AsimovDeploy.WinAgent.Framework.Models.UnitActions
{
    public class PowerShellUnitAction : UnitAction
    {
        public string Arguments { get; set; }

        public override AsimovTask GetTask(DeployUnit unit, AsimovUser user)
        {
            return new PowershellCommandTask(Arguments);
        }
    }
}