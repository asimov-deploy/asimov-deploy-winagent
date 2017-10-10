/*******************************************************************************
* Copyright (C) 2012 eBay Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
******************************************************************************/

using System;
using System.Net;
using System.Text;
using System.Threading;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.LoadBalancers;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Web.Contracts;
using log4net;
using Newtonsoft.Json;

namespace AsimovDeploy.WinAgent.Framework.Heartbeat
{
    public class HeartbeatService : IStartable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HeartbeatService));
        private Timer _timer;
        private readonly int _intervalMs;
        private readonly string _hostControlUrl;
        private readonly IAsimovConfig _config;
        private readonly ILoadBalancerService _loadBalancerService;

        public HeartbeatService(IAsimovConfig config, ILoadBalancerService loadBalancerService)
        {
            _config = config;
            _loadBalancerService = loadBalancerService;
            _intervalMs = config.HeartbeatIntervalSeconds * 1000;
            _hostControlUrl = config.WebControlUrl.ToString();
            _config = config;
            _config.ApiKey = Guid.NewGuid().ToString();
        }

        public void Start() => _timer = new Timer(TimerTick, null, 0, _intervalMs);

        public void Stop() => _timer.Dispose();

        private Uri GetHeartbeatUri(string nodeFrontUrl) => new Uri(new Uri(nodeFrontUrl), "/agent/heartbeat");

        private void TimerTick(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                SendHeartbeat();
            }
            finally
            {
                _timer.Change(_intervalMs, _intervalMs);
            }
        }

        private void SendHeartbeat()
        {
            HttpPostJsonUpdate(GetHeartbeatUri(_config.NodeFrontUrl), new HeartbeatDTO
            {
                name = Environment.MachineName,
                osPlatform = "Windows",
                groups = _config.GetAgentGroups(),
                url = _hostControlUrl,
                apiKey = _config.ApiKey,
                version = VersionUtil.GetAgentVersion(),
                configVersion = _config.ConfigVersion,
                loadBalancerState = _loadBalancerService.UseLoadBalanser ? _loadBalancerService.GetCurrentState() : null
            });
        }

        private static void HttpPostJsonUpdate<T>(Uri uri, T data)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.ContentType = "application/json";
            request.Method = "POST";
            request.Accept = "application/json";
            request.ProtocolVersion = HttpVersion.Version11;
            request.KeepAlive = false;
            request.Timeout = 5000;
            request.Accept = "*/*";
            request.ReadWriteTimeout = 5000;

            var parameters = JsonConvert.SerializeObject(data);
            var bytes = Encoding.UTF8.GetBytes(parameters);
            try
            {
                request.ContentLength = bytes.Length;
                using (var os = request.GetRequestStream())
                {
                    os.Write(bytes, 0, bytes.Length);
                }
                ((HttpWebResponse)request.GetResponse()).Close();

            }
            catch (WebException e)
            {
                Log.Warn("Error sending heartbeat to NodeFront", e);
            }
        }
    }
}