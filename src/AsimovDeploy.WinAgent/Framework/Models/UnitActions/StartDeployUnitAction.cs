using System;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using log4net;

namespace AsimovDeploy.WinAgent.Framework.Models.UnitActions
{
	public class StartDeployUnitAction : UnitAction
	{
        private static ILog _log = LogManager.GetLogger(typeof(StartDeployUnitAction));

		public StartDeployUnitAction()
		{
			Name = "Start";
		}

        public override AsimovTask GetTask(DeployUnit unit, AsimovUser user, string correlationId)
		{
			if (!(unit is ICanBeStopStarted))
				throw new ArgumentException("Action is only supported for deploy units that implement ICanBeStopStarted");

			return ((ICanBeStopStarted) unit).GetStartTask();
		}
	}

	
}