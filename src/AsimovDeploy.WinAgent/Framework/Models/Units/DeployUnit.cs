/*******************************************************************************
* Copyright (C) 2012 eBay Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Events;
using AsimovDeploy.WinAgent.Framework.Models.PackageSources;
using AsimovDeploy.WinAgent.Framework.Models.UnitActions;

namespace AsimovDeploy.WinAgent.Framework.Models.Units
{
    public abstract class DeployUnit
    {
        public string Name { get; set; }
        public PackageInfo PackageInfo { get; set; }
        public string DataDirectory { get; set; }
      
        public DeployStatus DeployStatus { get; protected set; }
        public DeployedVersion Version { get; protected set; }
        public string[] OnlyOnAgents { get; set; }
      
        public UnitActionList Actions { get;set; }
        
        public ActionParameterList DeployParameters { get; protected set; }
        public bool HasDeployParameters { get { return DeployParameters.Count > 0; } }

        protected DeployUnit()
        {
            DeployParameters = new ActionParameterList();
            Actions = new UnitActionList();
            Actions.Add(new RollbackUnitAction());
        }

        public abstract AsimovTask GetDeployTask(AsimovVersion version, ParameterValues parameterValues, AsimovUser user, string correlationId);

        public virtual DeployUnitInfo GetUnitInfo()
        {
            var deployUnitInfo = new DeployUnitInfo();
            deployUnitInfo.Name = Name;
            deployUnitInfo.HasDeployParameters = HasDeployParameters;

            if (Version == null)
            {
                Version = VersionUtil.GetCurrentVersion(DataDirectory);
                if (Version.DeployFailed)
                {
                    DeployStatus = DeployStatus.DeployFailed;
                }
            }

            if (!Version.DeployFailed)
            {
                deployUnitInfo.LastDeployed = string.Format("Deployed by {0} {1}", Version.UserName, DateUtils.GetFriendlyAge(Version.DeployTimestamp));
            }

            deployUnitInfo.Version = Version;
            deployUnitInfo.DeployStatus = DeployStatus;

            return deployUnitInfo;
        }

        public IList<DeployedVersion> GetDeployedVersions()
        {
            return VersionUtil.ReadVersionLog(DataDirectory);
        }

        public bool IsValidForAgent(string agentName)
        {
            if (OnlyOnAgents == null)
                return true;

            return OnlyOnAgents.Any(x => x == agentName);
        }

        public void StartingDeploy(AsimovVersion newVersion, string logFileName, AsimovUser user, string correlationId, ParameterValues parameters)
        {
            DeployStatus = DeployStatus.Deploying;
            Version = new DeployedVersion()
            {
                DeployTimestamp = DateTime.Now,
                VersionId = newVersion.Id,
                VersionNumber = newVersion.Number,
                VersionBranch = newVersion.Branch,
                VersionTimestamp = newVersion.Timestamp,
                VersionCommit = newVersion.Commit,
                LogFileName = logFileName,
				UserId = user.UserId,
				UserName = user.UserName,
                DeployFailed = false,
                CorrelationId = correlationId,
                Parameters = parameters.GetInternalDictionary()
            };

            NotificationPublisher.PublishNotifications(new DeployStartedEvent(Name, Version));
        }

        public void DeployCompleted()
        {
            DeployStatus = DeployStatus.NA;
            
			VersionUtil.UpdateVersionLog(DataDirectory, Version);
            
			var unitInfo = GetUnitInfo();
            NotificationPublisher.PublishNotifications((new DeployCompletedEvent(Name, Version, unitInfo.Status)));
        }

        public void DeployFailed()
        {
            DeployStatus = DeployStatus.DeployFailed;
            Version.DeployFailed = true;

            VersionUtil.UpdateVersionLog(DataDirectory, Version);

            NotificationPublisher.PublishNotifications(new DeployFailedEvent(Name, Version));
        }
    }
}