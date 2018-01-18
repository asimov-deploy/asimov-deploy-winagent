using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;
using Google.Cloud.Storage.V1;

namespace AsimovDeploy.WinAgentUpdater 
{
    public class UpdateInfoCollectorGCP
    {
        private static ILog _log = LogManager.GetLogger(typeof(UpdateInfoCollectorGCP));

        private readonly string _watchFolder;
        private readonly int _port;
        private readonly string bucketName;

        public UpdateInfoCollectorGCP(string watchFolder, int port)
        {
            _port = port;
            _watchFolder = watchFolder;
            bucketName = ParsePath();
        }
        private string ParsePath()
        {
            var regexgs = new Regex(@"(gs:)//(?<bucket>\S*)/", RegexOptions.IgnoreCase);
            var matchgs = regexgs.Match(_watchFolder);
            return matchgs.Result("${bucket}");
        }

        public UpdateInfo Collect()
        {
            return new UpdateInfo()
            {
                LastBuild = GetLatestVersion(),
                LastConfig = GetLatestBuild(),
                Current = new CurrentBuild(_port).GetCurrentBuild()
            };
        }

        private AsimovConfigUpdate GetLatestBuild()
        {
            var storage = StorageClient.Create();

            var regex = new Regex(@"(?<fileName>AsimovDeploy.WinAgent.ConfigFiles-Version-(\d+).zip)");
            var list = new List<AsimovConfigUpdate>();

            foreach (var bucket in storage.ListObjects(bucketName, "WinagentPackages"))
            {
                var match = regex.Match(bucket.Name);
                if (match.Success)
                {

                    list.Add(new AsimovConfigUpdate()
                    {
                        Version = int.Parse(match.Groups[1].Value),
                        FileSource = new GcpAsimovFileSource(bucket, storage)
                    });
                }
            }

            return list.OrderByDescending(x => x.Version).FirstOrDefault();
        }

        private AsimovVersion GetLatestVersion()
        {
            var storage = StorageClient.Create();

            var pattern = @"v(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)";
            var regex = new Regex(pattern);
            var list = new List<AsimovVersion>();
            foreach (var bucket in storage.ListObjects(bucketName, "WinagentPackages"))
            {
                var match = regex.Match(bucket.Name);
                if (match.Success)
                {
                    list.Add(new AsimovVersion()
                    {
                        Version = new Version(int.Parse(match.Groups["major"].Value), int.Parse(match.Groups["minor"].Value), int.Parse(match.Groups["build"].Value)),
                        FileSource = new GcpAsimovFileSource(bucket, storage)
                    });
                }
            }

            return list.OrderByDescending(x => x.Version).FirstOrDefault();
        }        
    }
}