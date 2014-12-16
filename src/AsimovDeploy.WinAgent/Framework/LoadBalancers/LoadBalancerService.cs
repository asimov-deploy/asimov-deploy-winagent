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
		private Uri loadbalancerAgentUri;
		
		public bool UseLoadBalanser { get; set; }

		public LoadBalancerService(IAsimovConfig config)
		{
			_config = config;

			if (config.LoadBalancerAgentUrl != null)
			{
				loadbalancerAgentUri = new Uri(config.LoadBalancerAgentUrl);
				UseLoadBalanser = true;
			}
		}

		public LoadBalancerStateDTO GetCurrentState()
		{
			try
			{
				var http = new HttpClient();
				var uri = new Uri(loadbalancerAgentUri, "ServerStatus");

                var result = http.Get(string.Format("{0}?server={1}&{2}", uri, _config.LoadBalancerServerId, _config.GetLoadBalancerParametersAsQueryString()));
				dynamic obj = result.DynamicBody;

				return new LoadBalancerStateDTO()
				{
					serverId = _config.LoadBalancerServerId,
					connectionCount = 0, //Todo: get number of connections from web
					enabled = obj.Status == "active"
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
			var uri = new Uri(loadbalancerAgentUri, "UpdateStatus");

		    var data = new ExpandoObject() as IDictionary<string, object>;
		    data.Add("server", _config.LoadBalancerServerId);
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