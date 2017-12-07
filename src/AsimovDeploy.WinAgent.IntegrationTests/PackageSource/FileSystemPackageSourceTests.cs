using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models.PackageSources;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.IntegrationTests.PackageSource
{
    public class FileSystemPackageSourceTests
    {
        private FileSystemPackageSource source;

        [SetUp]
        public void init()
        {
            source = new FileSystemPackageSource()
            {
                Name = "test",
                Pattern = "Package(?<version>\\d)\\.zip",
                Uri = new Uri($"file://{TestContext.CurrentContext.TestDirectory}/PackageSource/Sources")
            };
        }

        [Test]
        public void can_get_available_versions()
        {
            source.GetAvailableVersions(new PackageInfo()).Count.ShouldBe(2);
        }


        [Test]
        public void can_get_available_versions_in_source_relative_path()
        {
            var versions = source.GetAvailableVersions(new PackageInfo() {SourceRelativePath = "Source2"});
            versions.Count.ShouldBe(1);
            versions[0].Number.ShouldBe("2");
            versions[0].Id.ShouldBe("Source2\\Package2.zip");
        }
    }
}
