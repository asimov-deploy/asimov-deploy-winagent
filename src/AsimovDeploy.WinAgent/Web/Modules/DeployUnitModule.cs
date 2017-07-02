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

using System.Collections.Generic;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Web.Contracts;
using Nancy;
using Nancy.ModelBinding;

namespace AsimovDeploy.WinAgent.Web.Modules
{
    public class DeployUnitModule : NancyModule
    {
        public DeployUnitModule(IAsimovConfig config)
        {
            Get["/units/list"] = _ =>
            {
                var request = this.Bind<GetDeployUnitsRequestDto>();
                var units = GetDeployUnits(config, request);
                return Response.AsJson(units);
            };

            Get["/units/list/{group}"] = urlArgs =>
            {
                var units = GetDeployUnits(config, new GetDeployUnitsRequestDto
                {
                    AgentGroups = new[] { (string)urlArgs.group }
                });
                return Response.AsJson(units);
            };

            Get["/units/deploy-parameters/{unitName}"] = urlArgs =>
            {
                var deployUnit = config.GetUnitByName((string)urlArgs.unitName);
                if (deployUnit == null)
                    return 404;

                var parameters = deployUnit.GetDeployParameters().Select(deployParameter => deployParameter.GetDescriptor()).ToList();

                return Response.AsJson(parameters);
            };

            Get["/agent-groups"] = _ => Response.AsJson(config.GetAgentGroups());
            Get["/unit-groups"] = _ => Response.AsJson(config.GetUnitGroups());
            Get["/unit-types"] = _ => Response.AsJson(config.GetUnitTypes());
            Get["/unit-tags"] = _ => Response.AsJson(config.GetUnitTags());
        }

        private static List<DeployUnitInfoDTO> GetDeployUnits(IAsimovConfig config, GetDeployUnitsRequestDto getDeployUnitsRequestDto)
        {
            var deployUnits = config.Units.AsEnumerable();

            if (getDeployUnitsRequestDto.AgentGroups != null && getDeployUnitsRequestDto.AgentGroups.Any())
            {
                var filteredByAgentGroups = getDeployUnitsRequestDto.AgentGroups.SelectMany(config.GetUnitsByAgentGroup);
                deployUnits = deployUnits.Intersect(filteredByAgentGroups);
            }

            if (getDeployUnitsRequestDto.UnitGroups != null && getDeployUnitsRequestDto.UnitGroups.Any())
            {
                var filteredByUnitGroups = getDeployUnitsRequestDto.UnitGroups.SelectMany(config.GetUnitsByUnitGroup);
                deployUnits = deployUnits.Intersect(filteredByUnitGroups);
            }

            if (getDeployUnitsRequestDto.UnitTypes != null && getDeployUnitsRequestDto.UnitTypes.Any())
            {
                var filteredByType = getDeployUnitsRequestDto.UnitTypes.SelectMany(config.GetUnitsByType);
                deployUnits = deployUnits.Intersect(filteredByType);
            }

            if (getDeployUnitsRequestDto.Tags != null && getDeployUnitsRequestDto.Tags.Any())
            {
                var filteredByTags = getDeployUnitsRequestDto.Tags.SelectMany(config.GetUnitsByTag);
                deployUnits = deployUnits.Intersect(filteredByTags);
            }

            if (getDeployUnitsRequestDto.Units != null && getDeployUnitsRequestDto.Units.Any())
            {
                var filteredByUnits = getDeployUnitsRequestDto.Units.SelectMany(config.GetUnitsByUnitName);
                deployUnits = deployUnits.Intersect(filteredByUnits);
            }

            var units = new List<DeployUnitInfoDTO>();

            foreach (var deployUnit in deployUnits.ToList().Distinct())
            {
                var unitInfo = deployUnit.GetUnitInfo();
                var unitInfoDto = new DeployUnitInfoDTO
                {
                    name = unitInfo.Name,
                    group = unitInfo.Group,
                    type = deployUnit.UnitType,
                    lastDeployed = unitInfo.LastDeployed,
                    tags = deployUnit.Tags.ToArray()
                };

                if (unitInfo.DeployStatus != DeployStatus.NA)
                {
                    unitInfoDto.status = unitInfo.DeployStatus.ToString();
                    unitInfoDto.lastDeployed = "";
                }
                else
                {
                    unitInfoDto.status = unitInfo.Status.ToString();
                }

                unitInfoDto.url = unitInfo.Url;
                unitInfoDto.version = unitInfo.Version.VersionNumber;
                unitInfoDto.branch = unitInfo.Version.VersionBranch;
                unitInfoDto.hasDeployParameters = unitInfo.HasDeployParameters;
                unitInfoDto.actions = deployUnit.Actions.OrderBy(x => x.Sort).Select(x => x.Name).ToArray();

                units.Add(unitInfoDto);
            }

            return units;
        }
    }
}