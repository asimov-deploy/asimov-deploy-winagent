using System;
using System.Net;
using System.Threading.Tasks;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Web.Contracts;

namespace AsimovDeploy.WinAgent.Framework.LoadBalancers
{
    public class ServerMonitorLoadBalancer : ILoadBalancerService
    {
        private readonly string _url;
        private readonly int _loadBalancerDelaySeconds;
        private int _simulatedConnectionCount;

        public ServerMonitorLoadBalancer(IAsimovConfig config)
        {
            _url = config.LoadBalancerAgentUrl;
            _loadBalancerDelaySeconds = config.ServerMonitorLoadBalancerDelaySeconds;
        }

        public bool UseLoadBalanser { get; set; } = true;
        public LoadBalancerStateDTO GetCurrentState()
        {
            var health = LoadBalancerGet("health");

            return new LoadBalancerStateDTO
            {
                enabled = health && _simulatedConnectionCount == 0,
                connectionCount = _simulatedConnectionCount
            };
        }

        public void EnableServer()
        {
            if (LoadBalancerGet("")) return;

            LoadBalancerSet("in");
            Delay();
        }

        public void DisableServer()
        {
            if (!LoadBalancerGet("")) return;

            LoadBalancerSet("out");
            Delay();
        }

        private void Delay()
        {
            _simulatedConnectionCount = 1;
            Task.Delay(TimeSpan.FromSeconds(_loadBalancerDelaySeconds)).ContinueWith(task => { _simulatedConnectionCount = 0; });
        }

        private bool LoadBalancerGet(string op)
        {
            try
            {
                var req = WebRequest.Create($"{_url}/loadbalancer/{op}");
                req.GetResponse().Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void LoadBalancerSet(string op)
        {
            var req = WebRequest.Create($"{_url}/loadbalancer/{op}");
            req.Method = "POST";
            req.ContentLength = 0;
            req.GetResponse().Close();
        }
    }
}