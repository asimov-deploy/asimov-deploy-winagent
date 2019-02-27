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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models.PackageSources;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using JsonFx.Json;
using Nancy.Helpers;

namespace AsimovDeploy.WinAgent.Framework.Models
{
    public class AsimovConfig : IAsimovConfig
    {
        private string _loadBalancerServerId;

        private int _loadBalancerTimeout;

        public AsimovConfig()
        {
            DataFolder = "Data";
            LoadBalancerParameters = new Dictionary<string, string>();
        }

        public string DataFolder { get; set; }
        public PackageSourceList PackageSources { get; set; }
        public string Environment { get; set; }
        public string AgentGroup { get; set; }

        public int HeartbeatIntervalSeconds { get; set; }
        public int WebPort { get; set; }
        public string ApiKey { get; set; }
        public int ConfigVersion { get; set; }
        public string LoadBalancerType { get; set; }

        public string TempFolder => Path.Combine(DataFolder, "Temp");
        public string DownloadFolder => Path.Combine(DataFolder, "Download");

        public string LoadBalancerAgentUrl { get; set; }
        public string LoadBalancerServerId
        {
            get => _loadBalancerServerId ?? System.Environment.MachineName.ToLowerInvariant();
            set => _loadBalancerServerId = value;
        }
        public int LoadBalancerTimeout
        {
            get => _loadBalancerTimeout > 0 ? _loadBalancerTimeout : 30;
            set => _loadBalancerTimeout = value;
        }
        public int ServerMonitorLoadBalancerDelaySeconds { get; set; }

        public string NodeFrontUrl { get; set; }
        public string WebNotificationUrl { get; set; }

        public DeployUnits Units { get; set; } = new DeployUnits();
        public List<DeployEnvironment> Environments { get; set; } = new List<DeployEnvironment>();

        public DeployUnits GetUnitsByAgentGroup(string agentGroup = null)
        {
            if (AgentGroupIsSuppliedButNoMatchingFound(agentGroup))
                return new DeployUnits();
            return string.IsNullOrWhiteSpace(agentGroup) ?
                Units :
                new DeployUnits(Environments.First(a => a.AgentGroup == agentGroup).Units.Select(a => Units.First(b => b.Name == a.Name)));
        }

        public DeployUnits GetUnitsByUnitGroup(string unitGroup) => new DeployUnits(Units.Where(x => x.Group == unitGroup));

        public DeployUnits GetUnitsByType(string unitType) => new DeployUnits(Units.Where(x => x.UnitType == unitType));

        public DeployUnits GetUnitsByTag(string tag) => new DeployUnits(Units.Where(x => x.Tags.Any(t => t == tag)));

        public DeployUnits GetUnitsByUnitName(string unitName)
        {
            if (string.IsNullOrWhiteSpace(unitName)) return new DeployUnits();

            try
            {
                if (!unitName.StartsWith("^"))
                {
                    unitName = "^" + unitName;
                }

                if (!unitName.EndsWith("$"))
                {
                    unitName = unitName + "$";
                }

                var regex = new Regex(unitName, RegexOptions.IgnoreCase);

                return new DeployUnits(Units.Where(x => regex.IsMatch(x.Name)));
            }
            catch (ArgumentException)
            {
                return new DeployUnits();
            }
        }

        public DeployUnits GetUnitsByStatus(string status, bool refreshUnitStatus)
        {
            return new DeployUnits(Units.Where(x => x.GetUnitInfo(refreshUnitStatus).GetUnitStatus() == status));
        }

        public string[] GetAgentGroups()
        {
            return Environments
                .Where(x => x.AgentGroup != null)
                .Select(x => x.AgentGroup)
                .Distinct()
                .OrderBy(x => x).ToArray();
        }

        public string[] GetUnitGroups() =>
            Units
            .Where(x => x.Group != null)
            .Select(x => x.Group)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        public string[] GetUnitTypes() =>
            Units
            .Where(x => x.UnitType != null)
            .Select(x => x.UnitType)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        public string[] GetUnitTags() =>
            Units
            .Where(x => x.Tags != null)
            .SelectMany(x => x.Tags)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        public string[] GetUnitStatuses()
        {
            var statuses = Enum.GetValues(typeof(UnitStatus))
                .Cast<UnitStatus>()
                .Select(x => x.ToString());

            var deployStatuses = Enum.GetValues(typeof(DeployStatus))
                .Cast<DeployStatus>()
                .Select(x => x.ToString());

            return statuses
                .Concat(deployStatuses)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }

        public Uri WebControlUrl => new Uri($"http://{HostNameUtil.GetFullHostName()}:{WebPort}");

        public Dictionary<string, string> LoadBalancerParameters { get; set; }

        public string GetLoadBalancerParametersAsQueryString()
        {
            return LoadBalancerParameters.Count == 0 ? "" : string.Join("&", LoadBalancerParameters.Select(p => $"{HttpUtility.UrlEncode(p.Key.ToLower())}={HttpUtility.UrlEncode(p.Value.ToLower())}"));
        }

        public PackageSource GetPackageSourceFor(DeployUnit deployUnit)
        {
            if (deployUnit.PackageInfo?.Source == null)
            {
                return new NullPackageSource();
            }
            var matchingPackageSources = PackageSources.Where(x => x.Name == deployUnit.PackageInfo.Source).ToList();
            if (matchingPackageSources.Count == 0)
            {
                throw new ConfigurationErrorsException($"The package source {deployUnit.PackageInfo.Source}, used by the deploy unit {deployUnit.Name}, does not exist");
            }
            if (matchingPackageSources.Count > 1)
            {
                throw new ConfigurationErrorsException($"The package source {deployUnit.PackageInfo.Source}, used by the deploy unit {deployUnit.Name}, is declared more than once.");

            }
            return matchingPackageSources.Single();
        }

        public DeployUnit GetUnitByName(string name) => Units.First(x => x.Name == name);

        private bool AgentGroupIsSuppliedButNoMatchingFound(string agentGroup)
            => !string.IsNullOrWhiteSpace(agentGroup) && Environments.All(a => a.AgentGroup != agentGroup);
    }
}