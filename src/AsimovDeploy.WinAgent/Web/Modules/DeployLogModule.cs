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

using System.IO;
using System.Linq;
using System.Text;
using AsimovDeploy.WinAgent.Framework.Models;
using Nancy;
using Nancy.Responses;

namespace AsimovDeploy.WinAgent.Web.Modules
{
    public class DeployLogModule : NancyModule
    {
        public DeployLogModule(IAsimovConfig config)
        {

            Get["/deploylog/list/{unitName}"] = parameters =>
            {
                var deployUnit = config.GetUnitByName((string)parameters.unitName);
                if (deployUnit == null)
                    return 404;

                var deployedVersions = deployUnit.GetDeployedVersions();
                int position = 0;
                var jsonData = deployedVersions.Select(x => new
                {
                    timestamp = x.DeployTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    version = x.VersionNumber,
                    commit = x.VersionCommit,
                    branch = x.VersionBranch,
                    status = x.DeployFailed == true ? "DeployFailed" : "Success",
					userId = x.UserId,
					userName = x.UserName,
                    position = position++,
                });

                return Response.AsJson(jsonData);
            };

            Get["/deploylog/file/{unitName}/{position}"] = parameters =>
            {
                var deployUnit = config.GetUnitByName((string)parameters.unitName);
                if (deployUnit == null)
                    return 404;

                var deployedVersions = deployUnit.GetDeployedVersions();
                var specific = deployedVersions.ElementAtOrDefault((int)parameters.position);
                if (specific == null)
                    return 404;

                var logFile = Path.Combine(deployUnit.DataDirectory, "Logs", specific.LogFileName);
                using (var fileStream = new StreamReader(logFile, Encoding.UTF8))
                {
					return new TextResponse(fileStream.ReadToEnd(), "text/plain; charset=utf-8", Encoding.UTF8);
                }
            };
        }
    }
}