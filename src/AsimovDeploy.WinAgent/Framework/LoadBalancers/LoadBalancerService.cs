using System;
using System.Net;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Web.Contracts;
using EasyHttp.Http;
using JsonFx.Json;
using log4net;

namespace AsimovDeploy.WinAgent.Framework.LoadBalancers
{
	public class LoadBalancerService : ILoadBalancerService
	{
		private static ILog _log = LogManager.GetLogger(typeof (LoadBalancerService));

		private readonly IAsimovConfig _config;
		private Uri _agentUri;

		public LoadBalancerService(IAsimovConfig config)
		{
			_config = config;
			_agentUri = new Uri(config.LoadBalancer.LoadBalancerAgentUrl);
		}

		public LoadBalancerStateDTO GetCurrentState()
		{
			try
			{
				var http = new HttpClient();
				var uri = new Uri(_agentUri, "server-status");

				var result = http.Get(uri.ToString(), new { serverId = _config.LoadBalancer.ServerId });
				dynamic obj = result.DynamicBody;

				return new LoadBalancerStateDTO()
				{
					serverId = _config.LoadBalancer.ServerId,
					connectionCount = (int)obj.connections,
					enabled = obj.status == "enabled"
				};
			}
			catch (Exception ex)
			{
				_log.Error("Failed to get slb state", ex);
				return null;
			}
		}

		public void EnableServer()
		{
			ChangeServerStatus("enable");
		}

		public void DisableServer()
		{
			ChangeServerStatus("disable");
		}

		private void ChangeServerStatus(string status)
		{
			var http = new HttpClient();
			var uri = new Uri(_agentUri, "update-status");
			var data = new InternalUpdateStatusCommand
			{
				ServerId = _config.LoadBalancer.ServerId,
				Status = status
			};

			var result = http.Post(uri.ToString(), data, HttpContentTypes.ApplicationJson);
			if (result.StatusCode != HttpStatusCode.OK)
			{
				throw new LoadBalancerCommunicationException("Failed to enable server in load balancer, response: " + result.RawText);
			}
		}

		private class InternalUpdateStatusCommand
		{
			[JsonName("serverId")] 
			public string ServerId { get; set; }
			
			[JsonName("status")] 
			public string Status { get; set; }
		}
	}
}