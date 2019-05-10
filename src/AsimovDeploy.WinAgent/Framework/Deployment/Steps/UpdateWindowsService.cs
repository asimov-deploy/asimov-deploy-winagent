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
using System.Linq;
using System.ServiceProcess;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using AsimovDeploy.WinAgent.Framework.Tasks.ServiceControl;

namespace AsimovDeploy.WinAgent.Framework.Deployment.Steps
{
    public class UpdateWindowsService : IDeployStep
    {
        private IAsimovConfig _config;
        private static readonly TimeSpan DefaultStopTimeout = TimeSpan.FromMinutes(2);

        public UpdateWindowsService(IAsimovConfig config)
        {
            _config = config;
        }

        public void Execute(DeployContext context)
        {
            var deployUnit = (WindowsServiceDeployUnit) context.DeployUnit;

            using (var controller = new ServiceController(deployUnit.ServiceName))
            {
                StopService(context, controller);

                context.PhysicalPath = WindowsServiceUtil.GetWindowsServicePath(deployUnit.ServiceName);

                CleanPhysicalPath(context);

                CopyNewFiles(context);

                context.Log.InfoFormat("Starting service {0}", deployUnit.ServiceName);
                controller.Start();

                if(!context.ParameterValues.HasValue("SkipWaitForServiceStart",true))
                    controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
            }
        }

        private static void StopService(DeployContext context, ServiceController controller)
        {
            var killProcess = context.ParameterValues.Any(a => a.Key == "ForceKillServiceProcess");

            context.Log.InfoFormat("Stopping service {0} {1}", controller.ServiceName, killProcess ? "forcefully" : "");

            if (killProcess)
            {
                controller.KillServiceProcess();
            }
            else
            {
                if (controller.Status == ServiceControllerStatus.Running)
                    controller.StopServiceAndWaitForExit(DefaultStopTimeout);
            }
        }

        private void CopyNewFiles(DeployContext context)
        {
            context.Log.InfoFormat("Copying new files");
            DirectoryUtil.CopyDirectory(context.TempFolderWithNewVersionFiles, context.PhysicalPath);
        }

        private void CleanPhysicalPath(DeployContext context)
        {
            context.Log.InfoFormat("Cleaning folder {0}", context.PhysicalPath);
            DirectoryUtil.Clean(context.PhysicalPath);
        }
    }
}