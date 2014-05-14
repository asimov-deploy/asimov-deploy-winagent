using System;
using System.Collections.Generic;
using System.Dynamic;
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
		
		public bool UseLoadBalanser { get; set; }

		public LoadBalancerService(IAsimovConfig config)
		{
			_config = config;

			if (config.LoadBalancerAgentUrl != null)
			{
				_agentUri = new Uri(config.LoadBalancerAgentUrl);
				UseLoadBalanser = true;
			}
		}

		public LoadBalancerStateDTO GetCurrentState()
		{
			try
			{
				var http = new HttpClient();
				var uri = new Uri(_agentUri, "server-status");

                var result = http.Get(string.Format("{0}?name={1}&{2}", uri, _config.LoadBalancerServerId, _config.GetLoadBalancerParametersAsQueryString()));
				dynamic obj = result.DynamicBody;

				return new LoadBalancerStateDTO()
				{
					serverId = _config.LoadBalancerServerId,
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

		    var data = new ExpandoObject() as IDictionary<string, object>;
		    data.Add("name", _config.LoadBalancerServerId);
            data.Add("status", status);

		    foreach (var parameter in _config.LoadBalancerParameters)
		    {
		        data.Add(parameter.Key.ToLower(), parameter.Value);
		    }

			var result = http.Post(uri.ToString(), data, HttpContentTypes.ApplicationJson);
			if (result.StatusCode != HttpStatusCode.OK)
			{
				throw new LoadBalancerCommunicationException("Failed to enable server in load balancer, response: " + result.RawText);
			}
		}
	}
}