namespace AsimovDeploy.WinAgent.Web.Contracts
{
	public class LoadBalancerStateDTO
	{
	    public override string ToString()
	    {
	        return $"enabled: {enabled}, Connection count: {connectionCount}";
	    }

	    public bool enabled;
		public int connectionCount;
		public string serverId;
	}
}