using System;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models.Units;

namespace AsimovDeploy.WinAgent.Framework.Deployment.Steps
{
    public class InstallWindowsService : IDeployStep
    {
        private readonly WindowsServiceInstallConfig service;

        public InstallWindowsService(WindowsServiceInstallConfig service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            this.service = service;
        }

        public void Execute(DeployContext context)
        {
            CopyFiles(context,service);
            InstallService(context, service);
        }

        private void InstallService(DeployContext context, WindowsServiceInstallConfig service)
        {
            ProcessUtil.ExecutePowershellScript(
                service.TargetPath, 
                service.Install, 
                context.ParameterValues, 
                context.Log);
        }

        private void CopyFiles(DeployContext context, WindowsServiceInstallConfig service)
        {
            context.Log.InfoFormat("Copying files");
            DirectoryUtil.CopyDirectory(context.TempFolderWithNewVersionFiles, service.TargetPath);
        }
    }
}