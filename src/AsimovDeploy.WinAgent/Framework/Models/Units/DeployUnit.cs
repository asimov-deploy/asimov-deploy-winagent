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
using System.Text.RegularExpressions;
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

        public UnitActionList Actions { get; set; } = new UnitActionList { new RollbackUnitAction() };
        public ActionParameterList DeployParameters { get; protected set; } = new ActionParameterList();
        public ActionParameterList Credentials { get; protected set; } = new ActionParameterList();

        public bool HasDeployParameters => GetDeployParameters().Count > 0 || GetCredentials().Count > 0;

        public string Group { get; set; } = "N/A";
        public abstract string UnitType { get; }

        public List<string> Tags { get; set; } = new List<string>
        {
            "os:Windows",
            $"host:{Environment.MachineName}"
        };

        public abstract AsimovTask GetDeployTask(AsimovVersion version, ParameterValues parameterValues, AsimovUser user, string correlationId);

        public virtual DeployUnitInfo GetUnitInfo(bool refreshUnitStatus)
        {
            var deployUnitInfo = new DeployUnitInfo
            {
                Name = Name,
                Group = Group,
                HasDeployParameters = HasDeployParameters,

            };

            if (!refreshUnitStatus)
            {
                Version = new DeployedVersion() { VersionNumber = "0.0.0.0" };
            }

            if (Version == null)
            {
                Version = VersionUtil.GetCurrentVersion(DataDirectory);
                if (Version.DeployFailed)
                    DeployStatus = DeployStatus.DeployFailed;
            }

            if (!Version.DeployFailed)
            {
                if (Version.DeployTimestamp == DateTime.MinValue)
                {
                    deployUnitInfo.LastDeployed = string.Empty;
                }
                else
                {
                    deployUnitInfo.LastDeployed = $"Deployed by {Version.UserName} {DateUtils.GetFriendlyAge(Version.DeployTimestamp)}";
                }
            }

            deployUnitInfo.Version = Version;
            deployUnitInfo.DeployStatus = DeployStatus;

            if (refreshUnitStatus)
            {
                UpdateUnitStatus();
            }

            return deployUnitInfo;
        }

        protected virtual void UpdateUnitStatus()
        {

        }

        public IList<DeployedVersion> GetDeployedVersions() => VersionUtil.ReadVersionLog(DataDirectory);

        public bool IsValidForAgent(string agentName)
        {
            if (OnlyOnAgents?.Any(x => x == agentName) ?? true)
                return true;

            return OnlyOnAgents.Where(t => t.Contains("*")).Any(agent => new Regex("^" + agent.Replace("*", ".*")).IsMatch(agentName));
        }

        public void StartingDeploy(AsimovVersion newVersion, string logFileName, AsimovUser user, string correlationId, ParameterValues parameters)
        {
            DeployStatus = DeployStatus.Deploying;
            Version = new DeployedVersion
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

            var unitInfo = GetUnitInfo(true);
            NotificationPublisher.PublishNotifications(new DeployCompletedEvent(Name, Version, unitInfo.Status));
        }

        public void DeployFailed()
        {
            DeployStatus = DeployStatus.DeployFailed;
            Version.DeployFailed = true;

            VersionUtil.UpdateVersionLog(DataDirectory, Version);

            NotificationPublisher.PublishNotifications(new DeployFailedEvent(Name, Version));
        }

        public virtual ActionParameterList GetDeployParameters()
        {
            return DeployParameters;
        }
        public virtual ActionParameterList GetCredentials()
        {
            return Credentials;
        }
    }
}