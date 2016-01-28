using AsimovDeploy.WinAgent.Framework.Models.PackageSources;

namespace AsimovDeploy.WinAgent.Framework.Models.Units
{
	public class DeployEnvironment
	{
		public DeployUnits Units { get; set; } = new DeployUnits();
		public PackageSourceList PackageSources { get; set; }
		public string AgentGroup { get; set; }
		public string NodeFrontUrl { get; set; }
	}
}