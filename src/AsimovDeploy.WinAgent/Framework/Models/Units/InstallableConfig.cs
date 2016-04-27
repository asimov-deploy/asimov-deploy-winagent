namespace AsimovDeploy.WinAgent.Framework.Models.Units
{
    public class InstallableConfig
    {
        public string TargetPath { get; set; }
        public string Install { get; set; }
        public ActionParameterList InstallParameters { get; set; }

        public string Uninstall { get; set; }

    }
}