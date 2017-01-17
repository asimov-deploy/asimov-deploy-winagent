using System;
using System.Collections.Generic;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.Units;

namespace AsimovDeploy.WinAgent.Framework.Deployment.Steps
{
    public class InstallWebSite : IDeployStep
    {
        private readonly IWebSiteDeployUnit unit;

        public InstallWebSite(IWebSiteDeployUnit unit)
        {
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            this.unit = unit;

        }

        public void Execute(DeployContext context)
        {
            CopyFiles(context);
            InstallService(context);
        }

        private void InstallService(DeployContext context)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "SiteName", unit.SiteName },
                { "SiteUrl", unit.SiteUrl }
            };
            foreach (var contextParameterValue in context.ParameterValues)
            {
                parameters[contextParameterValue.Key] = contextParameterValue.Value;
            }

            ProcessUtil.ExecutePowershellScript(
                unit.Installable.TargetPath,
                unit.Installable.Install,
                parameters,
                context.Log);
        }

        private void CopyFiles(DeployContext context)
        {
            context.Log.Info($"Copying files from {context.TempFolderWithNewVersionFiles} to {unit.Installable.TargetPath}...");
            DirectoryUtil.CopyDirectory(context.TempFolderWithNewVersionFiles, unit.Installable.TargetPath);
        }
    }
}