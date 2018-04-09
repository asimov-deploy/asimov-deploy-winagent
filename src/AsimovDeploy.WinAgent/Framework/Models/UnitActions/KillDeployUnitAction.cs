using System;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using log4net;

namespace AsimovDeploy.WinAgent.Framework.Models.UnitActions
{
    public class KillDeployUnitAction : UnitAction
    {
        private static ILog _log = LogManager.GetLogger(typeof(StopDeployUnitAction));

        public KillDeployUnitAction() => Name = "Kill";

        public override AsimovTask GetTask(DeployUnit unit, AsimovUser user, string correlationId)
        {
            if (!(unit is ICanBeKilled))
                throw new ArgumentException("Action is only supported for deploy units that implement ICanBeKilled");

            return ((ICanBeKilled)unit).GetKillTask();
        }
    }
}