namespace AsimovDeploy.WinAgent.Framework.Models.Units
{
    public class InstallableConfig
    {
        public string TargetPath { get; set; }
        public string Install { get; set; }
        public string AssemblyName { get; set; }
        public string ScriptsDir { get; set; }
        public ActionParameterList InstallParameters { get; set; } = new ActionParameterList();

        public string Uninstall { get; set; }

        public string InstallType { get; set; }
        public ActionParameterList Credentials { get; set; } = new ActionParameterList();

        public ActionParameterList GetInstallAndCredentialParameters()
        {
            var combined = new ActionParameterList();
            combined.AddRange(Credentials);
            combined.AddRange(InstallParameters);

            return combined;
        }

        public bool IsInstallable()
        {
            return Install != null || InstallType != null;
        }

        public bool IsUninstallable()
        {
            return !string.IsNullOrEmpty(Uninstall) || InstallType != null;
        }
    }
}