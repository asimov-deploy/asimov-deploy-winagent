using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace AsimovDeploy.WinAgent.Framework.Models.PackageSources
{
    public class AwsS3PackageSource : PackageSource
    {
        private AmazonS3Client _s3Client;
        private AmazonS3Client S3Client => _s3Client ?? (_s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(Region)));

        public string Region { get; set; }
        public string Bucket { get; set; }
        public string Prefix { get; set; }
        public string Pattern { get; set; } = @"v(?<version>\d+\.\d+\.\d+\.\d+)-\[(?<branch>[\w\-]*)\]-\[(?<commit>\w*)\]";


        public override IList<AsimovVersion> GetAvailableVersions(PackageInfo packageInfo)
        {
            var objects = S3Client.ListObjects(Bucket, Prefix);
            return objects.S3Objects.Select(x => ParseVersion(x.Key, x.LastModified)).Where(x => x != null).ToList();
        }

        private AsimovVersion ParseVersion(string key, DateTime xLastModified)
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
                Timestamp = xLastModified
            };
            return version;
        }

        public override AsimovVersion GetVersion(string versionId, PackageInfo packageInfo)
        {
            var @object = S3Client.GetObject(Bucket, versionId);
            return ParseVersion(@object.Key, @object.LastModified);
        }

        public override string CopyAndExtractToTempFolder(string versionId, PackageInfo packageInfo, string tempFolder)
        {
            var @object = S3Client.GetObject(Bucket, versionId);
            var localFileName = Path.Combine(tempFolder,versionId);
            @object.WriteResponseStreamToFile(localFileName);

            Extract(localFileName, tempFolder, packageInfo.InternalPath);

            File.Delete(localFileName);

            return Path.Combine(tempFolder, packageInfo.InternalPath);
        }
    }
}
