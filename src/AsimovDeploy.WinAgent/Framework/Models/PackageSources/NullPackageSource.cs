using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsimovDeploy.WinAgent.Framework.Models.PackageSources
{
    public class NullPackageSource : PackageSource
    {
        public override IList<AsimovVersion> GetAvailableVersions(PackageInfo packageInfo)
        {
            return new List<AsimovVersion>();
        }

        public override AsimovVersion GetVersion(string versionId, PackageInfo packageInfo)
        {
            throw new InvalidOperationException("An empty package source does not have any versions");
        }

        public override string CopyAndExtractToTempFolder(string versionId, PackageInfo packageInfo, string tempFolder)
        {
            throw new InvalidOperationException("An empty package source can not copy to temp folder");
        }
    }
}
