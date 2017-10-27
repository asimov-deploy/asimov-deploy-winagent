using System;
using System.Collections.Generic;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Configuration;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.UnitActions;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
    public class DeployAllUnitsTask : AsimovTask
    {
        private readonly string _preferredBranch;
        private readonly AsimovUser _user;
        private readonly string _correlationId;

        public DeployAllUnitsTask(string preferredBranch, AsimovUser user, string correlationId)
        {
            _preferredBranch = preferredBranch;
            _user = user;
            _correlationId = correlationId;
        }

        protected override void Execute()
        {
            foreach (var deployUnit in Config.Units)
            {
                var packageSource = Config.GetPackageSourceFor(deployUnit);
                var availableVersions = packageSource.GetAvailableVersions(deployUnit.PackageInfo);

                var version = availableVersions.FirstOrDefault(x => x.Branch.Equals(_preferredBranch, StringComparison.InvariantCultureIgnoreCase));

                if (version == null)
                {
                    version = availableVersions.FirstOrDefault(x => x.Branch.Equals("master", StringComparison.InvariantCultureIgnoreCase));
                }

                if (version == null)
                {
                    version = availableVersions.FirstOrDefault();
                }

                if (version == null)
                {
                    Log.Error($"No version to deploy found for unit {deployUnit.Name}");
                    continue;
                }

                var parameters = new Dictionary<string, object>();

                if (deployUnit.HasDeployParameters)
                {
                    foreach (var deployParameter in deployUnit.GetDeployParameters())
                    {
                        parameters.Add(deployParameter.Name, deployParameter.GetDescriptor().@default);   
                    }
                }

                var parameterValues = new ParameterValues(parameters);
                var deployTask = deployUnit.GetDeployTask(version, parameterValues, _user, _correlationId);

                deployTask.ExecuteTask();
            }

            var startableUnits = Config.GetUnitsByType(DeployUnitTypes.WindowsService).Concat(Config.GetUnitsByType(DeployUnitTypes.WebSite));

            foreach (var deployUnit in startableUnits)
            {
                var action = new StartDeployUnitAction();

                var task = action.GetTask(deployUnit, _user, _correlationId);

                try
                {
                    task?.ExecuteTask();
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to start unit {deployUnit.Name}", ex);
                }
            }
        }
    }
}