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
using AsimovDeploy.WinAgent.Framework.Models.PackageSources;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using Environment = AsimovDeploy.WinAgent.Framework.Models.Units.Environment;

namespace AsimovDeploy.WinAgent.Framework.Models
{
    public interface IAsimovConfig
    {
        string Environment { get; }
		string AgentGroup { get; set; }

		int HeartbeatIntervalSeconds { get; }
        string TempFolder { get; }
        string NodeFrontUrl { get;}
        string WebNotificationUrl { get; set; }
        int WebPort { get; }
        string ApiKey { get; set; }
        int ConfigVersion { get; }

		string LoadBalancerAgentUrl { get; set; }
		string LoadBalancerServerId { get; set; }
		int LoadBalancerTimeout { get; set; }

		List<Environment> Environments { get; set; }
		DeployUnits Units { get; set; }
		DeployUnit GetUnitByName(string name);
		DeployUnits GetUnitsByGroup(string agentGroup = null);

		Uri WebControlUrl { get; }
        Dictionary<string, string> LoadBalancerParameters { get; set; }

        PackageSource GetPackageSourceFor(DeployUnit deployUnit);
        string GetLoadBalancerParametersAsQueryString();

    }
}