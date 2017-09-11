namespace AsimovDeploy.WinAgent.Framework.Models.Units
{
    public interface IInstallableService : IInstallable
    {
        string ServiceName { get; set; }
    }
}