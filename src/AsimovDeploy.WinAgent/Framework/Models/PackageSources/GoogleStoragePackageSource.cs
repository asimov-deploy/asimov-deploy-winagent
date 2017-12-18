using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace AsimovDeploy.WinAgent.Framework.Models.PackageSources
{
    public class GoogleStoragePackageSource : PackageSource
    {
        private const int MaxPageSize = 100;
        private StorageClient _storageClient;
        private StorageClient StorageClient
        {
            get
            {
                if (_storageClient != null)
                    return _storageClient;
                var credentials = !string.IsNullOrEmpty(CredentialsJson)
                    ? GoogleCredential.FromJson(CredentialsJson)
                    : null;
                return _storageClient = StorageClient.Create(credentials);
            }
        }

        public string Region { get; set; }
        public string Bucket { get; set; }
        public string Prefix { get; set; }
        public string Pattern { get; set; } =
            @"v(?<version>\d+\.\d+\.\d+\.\d+)-\[(?<branch>[\w\-]*)\]-\[(?<commit>\w*)\]";
        public string CredentialsJson { get; set; }


        public override IList<AsimovVersion> GetAvailableVersions(PackageInfo packageInfo)
        {
            var prefix = packageInfo.SourceRelativePath != null ? $"{Prefix}/{packageInfo.SourceRelativePath}" : Prefix;
            var objects = StorageClient.ListObjects(Bucket, prefix);
            return objects.ReadPage(MaxPageSize).Select(x => ParseVersion(x.Name, x.Updated)).Where(x => x != null)
                .ToList();
        }

        private AsimovVersion ParseVersion(string key, DateTime? lastModified)
        {
            var match = Regex.Match(key, Pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            var version = new AsimovVersion
            {
                Id = key,
                Number = match.Groups["version"].Value,
                Branch = match.Groups["branch"].Value,
                Commit = match.Groups["commit"].Value,
                Timestamp = lastModified ?? DateTime.MinValue
            };
            return version;
        }

        public override AsimovVersion GetVersion(string versionId, PackageInfo packageInfo)
        {
            var @object = StorageClient.GetObject(Bucket, versionId);
            return ParseVersion(@object.Name, @object.Updated);
        }

        public override string CopyAndExtractToTempFolder(string versionId, PackageInfo packageInfo, string tempFolder)
        {
            var @object = StorageClient.GetObject(Bucket, versionId);
            var objectFileName = Path.GetFileName(versionId);
            if (objectFileName == null)
                throw new InvalidOperationException($"Could not extract file name from object {versionId}");
            var localFileName = Path.Combine(tempFolder, objectFileName);
            using (var fileStream = File.OpenWrite(localFileName))
            {
                StorageClient.DownloadObject(Bucket, versionId, fileStream);
            }

            Extract(localFileName, tempFolder, packageInfo.InternalPath);

            File.Delete(localFileName);

            return Path.Combine(tempFolder, packageInfo.InternalPath);
        }
    }
}