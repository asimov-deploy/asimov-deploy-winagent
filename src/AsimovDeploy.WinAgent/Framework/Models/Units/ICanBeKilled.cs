using AsimovDeploy.WinAgent.Framework.Common;

namespace AsimovDeploy.WinAgent.Framework.Models.Units
{
    public interface ICanBeKilled
    {
        AsimovTask GetKillTask();
    }
}