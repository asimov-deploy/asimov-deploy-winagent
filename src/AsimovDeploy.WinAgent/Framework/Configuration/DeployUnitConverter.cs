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
using System.Security;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Environment = System.Environment;

namespace AsimovDeploy.WinAgent.Framework.Configuration
{
    public class DeployUnitConverter : JsonConverter
    {
        static readonly ILog _logger = LogManager.GetLogger(typeof(DeployUnitConverter));

        public static IDictionary<string, Type> DeployUnitLookup;

        static DeployUnitConverter()
        {
            DeployUnitLookup = new Dictionary<string, Type>
            {
                { DeployUnitTypes.WebSite, typeof(WebSiteDeployUnit) },
                { DeployUnitTypes.Iis6WebSite, typeof(IIS6WebSiteDeployUnit) },
                { DeployUnitTypes.WindowsService, typeof(WindowsServiceDeployUnit) },
                { DeployUnitTypes.PowerShell, typeof(PowerShellDeployUnit) },
                { DeployUnitTypes.FileCopy, typeof(FileCopyDeployUnit) }
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var list = (DeployUnits)existingValue;

            foreach (JObject jsonUnit in JArray.Load(reader))
            {
                var type = jsonUnit.Property("Type");

                if (type == null)
                {
                    _logger.ErrorFormat("Missing property 'Type' for JSON deploy unit object. Continuing. The JSON value was: {0}.", jsonUnit);
                    continue;
                }

                if (!DeployUnitLookup.TryGetValue(type.Value.ToString(), out var deployUnitType))
                {
                    var alternatives = string.Join(", ", Enum.GetNames(typeof(DeployUnitTypes)));
                    _logger.ErrorFormat(
                        "Property 'Type' has invalid value for JSON deploy unit object. Please make it one of the following {{ {0} }}. Continuing. The JSON value was: {1}",
                        alternatives, jsonUnit);
                    continue;
                }

                var deployUnit = (DeployUnit)serializer.Deserialize(jsonUnit.CreateReader(), deployUnitType);

                if (deployUnit.IsValidForAgent(Environment.MachineName.ToLowerInvariant()))
                    list.Add(deployUnit);
            }

            return existingValue;
        }

        public override bool CanConvert(Type objectType) => false;
    }
}