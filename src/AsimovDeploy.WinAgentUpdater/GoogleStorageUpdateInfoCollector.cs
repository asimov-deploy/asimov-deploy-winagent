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
    public class GoogleStorageUpdateInfoCollector : IUpdateInfoCollector
    {
        static readonly Regex GoogleStorageUri = new Regex(@"(gs:)//(?<bucket>\S*)/(?<prefix>.*)", RegexOptions.IgnoreCase);

        private readonly int _port;
        private readonly string _bucket;
        private readonly string _prefix;

        private StorageClient _client;

        public GoogleStorageUpdateInfoCollector(string watchFolder, int port)
        {
            _port = port;
            var matchGoogleStorageUri = GoogleStorageUri.Match(watchFolder);
            if (!matchGoogleStorageUri.Success)
            {
                throw new ArgumentException("Watch folder is not a valid google storage uri. Expected an uri of the for gs://bucket/prefix, was " + watchFolder,nameof(watchFolder));
            }
            _bucket = matchGoogleStorageUri.Result("${bucket}");
            _prefix = matchGoogleStorageUri.Result("${prefix}");

            _client = StorageClient.Create();
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
            var regex = new Regex(@"(?<fileName>AsimovDeploy.WinAgent.ConfigFiles-Version-(\d+).zip)");
            var list = new List<AsimovConfigUpdate>();

            foreach (var bucket in _client.ListObjects(_bucket, _prefix + "/AsimovDeploy.WinAgent.ConfigFiles-Version"))
            {
                var match = regex.Match(bucket.Name);
                if (match.Success)
                {

                    list.Add(new AsimovConfigUpdate()
                    {
                        Version = int.Parse(match.Groups[1].Value),
                        FileSource = new GoogleStorageFileSource(bucket, _client)
                    });
                }
            }

            return list.OrderByDescending(x => x.Version).FirstOrDefault();
        }

        private AsimovVersion GetLatestVersion()
        {
            var pattern = @"v(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)";
            var regex = new Regex(pattern);
            var list = new List<AsimovVersion>();
            foreach (var bucket in _client.ListObjects(_bucket, _prefix + "/AsimovDeploy.WinAgent-v"))
            {
                var match = regex.Match(bucket.Name);
                if (match.Success)
                {
                    list.Add(new AsimovVersion
                    {
                        Version = new Version(int.Parse(match.Groups["major"].Value), int.Parse(match.Groups["minor"].Value), int.Parse(match.Groups["build"].Value)),
                        FileSource = new GoogleStorageFileSource(bucket, _client)
                    });
                }
            }

            return list.OrderByDescending(x => x.Version).FirstOrDefault();
        }        
    }
}