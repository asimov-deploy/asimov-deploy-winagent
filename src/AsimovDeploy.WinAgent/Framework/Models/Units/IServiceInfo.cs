namespace AsimovDeploy.WinAgent.Framework.Models.Units
{
    public interface IInstallableService
    {
        string ServiceName { get; set; }
        InstallableConfig Installable { get; set; }
    }
}