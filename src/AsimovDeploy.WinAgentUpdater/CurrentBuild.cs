using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsimovDeploy.WinAgentUpdater
{
    class CurrentBuild
    {
        private static ILog _log = LogManager.GetLogger(typeof(CurrentBuild));
        private readonly int _port;

        public CurrentBuild(int port)
        {           
            _port = port;
        }
        private static string GetFullHostName()
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            if (ipProperties.DomainName != string.Empty)
                return string.Format("{0}.{1}", ipProperties.HostName, ipProperties.DomainName);
            else
                return ipProperties.HostName;
        }

        public AgentVersionInfo GetCurrentBuild()
        {
            string url = String.Format("http://{0}:{1}/version", GetFullHostName(), _port);
            try
            {
                using (var response = WebRequest.Create(url).GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        using (var jsonReader = new JsonTextReader(reader))
                        {
                            var jObject = JObject.Load(jsonReader);
                            var version = (string)jObject.Property("version").Value;
                            var parts = version.Split('.');
                            return new AgentVersionInfo()
                            {
                                Version = new Version(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2])),
                                ConfigVersion = (int)jObject.Property("configVersion")
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Failed fetch version from: {0}", url);
                _log.Error(ex);
                return new AgentVersionInfo() { Version = new Version(0, 0, 0), ConfigVersion = 0 };
            }
        }
    }
}
