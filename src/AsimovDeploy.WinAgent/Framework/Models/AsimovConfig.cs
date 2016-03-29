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
using System.IO;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models.PackageSources;
using AsimovDeploy.WinAgent.Framework.Models.Units;
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

        public string TempFolder => Path.Combine(DataFolder, "Temp");

        public string LoadBalancerAgentUrl { get; set; }
        public string LoadBalancerServerId
        {
            get { return _loadBalancerServerId ?? System.Environment.MachineName.ToLower(); }
            set { _loadBalancerServerId = value; }
        }
        public int LoadBalancerTimeout
        {
            get { return _loadBalancerTimeout > 0 ? _loadBalancerTimeout : 30; }
            set { _loadBalancerTimeout = value; }
        }

        public string NodeFrontUrl { get; set; }
        public string WebNotificationUrl { get; set; }

		public DeployUnits Units { get; set; } = new DeployUnits();
		public List<DeployEnvironment> Environments { get; set; } = new List<DeployEnvironment>();

	    public DeployUnits GetUnitsByGroup(string agentGroup = null)
	    {
			if (!string.IsNullOrWhiteSpace(agentGroup) && !Environments.Any(a => a.AgentGroup == agentGroup)) return new DeployUnits();
			return string.IsNullOrWhiteSpace(agentGroup) ? Units : Environments.First(a => a.AgentGroup == agentGroup).Units;
		}

		public Uri WebControlUrl => new Uri($"http://{HostNameUtil.GetFullHostName()}:{WebPort}");
        public Dictionary<string, string> LoadBalancerParameters { get; set; }

        public string GetLoadBalancerParametersAsQueryString()
        {
            if (LoadBalancerParameters.Count == 0)
                return "";

            return string.Join("&", LoadBalancerParameters.Select(p => $"{HttpUtility.UrlEncode(p.Key.ToLower())}={HttpUtility.UrlEncode(p.Value.ToLower())}"));
        }

        public PackageSource GetPackageSourceFor(DeployUnit deployUnit)
        {
			return PackageSources.Single(x => x.Name == deployUnit.PackageInfo.Source);
        }

	    public DeployUnit GetUnitByName(string name) => Units.Single(x => x.Name == name);
    }
}