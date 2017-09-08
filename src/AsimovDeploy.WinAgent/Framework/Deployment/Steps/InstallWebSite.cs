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
        private readonly IWebSiteDeployUnit _unit;

        public InstallWebSite(IWebSiteDeployUnit unit)
        {
            _unit = unit ?? throw new ArgumentNullException(nameof(unit));
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
                { "SiteName", _unit.SiteName },
                { "SiteUrl", _unit.SiteUrl }
            };
            foreach (var contextParameterValue in context.ParameterValues)
            {
                parameters[contextParameterValue.Key] = contextParameterValue.Value;
            }

            ProcessUtil.ExecutePowershellScript(
                _unit.Installable.TargetPath,
                _unit.Installable.Install,
                parameters,
                context.Log, 
                new[] { _unit.Installable.ScriptsDir });
        }

        private void CopyFiles(DeployContext context)
        {
            context.Log.Info($"Copying files from {context.TempFolderWithNewVersionFiles} to {_unit.Installable.TargetPath}...");
            DirectoryUtil.CopyDirectory(context.TempFolderWithNewVersionFiles, _unit.Installable.TargetPath);
        }
    }
}