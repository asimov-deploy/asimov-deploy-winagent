using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Common;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using log4net;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace AsimovDeploy.WinAgent.Framework.Models.PackageSources
{
    public class GoogleStoragePackageSource : PackageSource
    {
        private const int MaxReturnedResults = 100;
        private StorageClient _storageClient;
        private static ILog _log = LogManager.GetLogger(typeof(GoogleStoragePackageSource));
        private StorageClient StorageClient
        {
            get
            {
                if (_storageClient != null)
                    return _storageClient;
                var credentials = !string.IsNullOrEmpty(Credentials)
                    ? GoogleCredential.FromJson(Credentials)
                    : null;
                return _storageClient = StorageClient.Create(credentials);
            }
        }

        public string Bucket { get; set; }
        public string Prefix { get; set; }
        public string Pattern { get; set; } =
            @"v(?<version>\d+\.\d+\.\d+\.\d+)-\[(?<branch>[\w\-]*)\]-\[(?<commit>\w*)\]";
        public string Credentials { get; set; }


        public override IList<AsimovVersion> GetAvailableVersions(PackageInfo packageInfo)
        {
            var prefix = packageInfo.SourceRelativePath != null ? $"{Prefix.TrimEnd('/')}/{packageInfo.SourceRelativePath}" : Prefix;
            var objects = StorageClient.ListObjects(Bucket, prefix);
            return objects
                .Select(ParseVersion)
                .Where(x => x != null)
                .OrderByDescending(x=>x.Timestamp)
                .Take(MaxReturnedResults)
                .ToList();
        }

        public override AsimovVersion GetVersion(string versionId, PackageInfo packageInfo)
        {
            var @object = StorageClient.GetObject(Bucket, versionId);
            return ParseVersion(@object);
        }

        private AsimovVersion ParseVersion(Object @object)
        {
            return AsimovVersion.Parse(Pattern,@object.Name, @object.TimeCreated ?? DateTime.MinValue);
        }

        public override string CopyAndExtractToTempFolder(string versionId, PackageInfo packageInfo, string tempFolder, string downloadFolder)
        {
            var objectFileName = Path.GetFileName(versionId);
            if (objectFileName == null)
                throw new InvalidOperationException($"Could not extract file name from object {versionId}");

            var localFileName = Path.Combine(downloadFolder, objectFileName);
            var metadata = StorageClient.GetObject(Bucket, versionId);

            if (DirectoryUtil.Exists(localFileName) && metadata.Md5Hash == DirectoryUtil.Md5(localFileName)) 
            {
                _log.Info("Using cached file: " + localFileName);
            }
            else
            {
                using (var fileStream = File.OpenWrite(localFileName))
                {
                    StorageClient.DownloadObject(Bucket, versionId, fileStream);
                }
            }
            
            Extract(localFileName, tempFolder, packageInfo.InternalPath);
            
            return Path.Combine(tempFolder, packageInfo.InternalPath);
        }
    }
}