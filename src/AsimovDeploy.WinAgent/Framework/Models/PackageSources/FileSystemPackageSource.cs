﻿/*******************************************************************************
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Common;

namespace AsimovDeploy.WinAgent.Framework.Models.PackageSources
{
    public class FileSystemPackageSource : PackageSource
    {
        public Uri Uri { get; set; }
        public string Pattern { get; set; } = AsimovVersion.DefaultPattern;

        public override IList<AsimovVersion> GetAvailableVersions(PackageInfo packageInfo)
        {
            var versions = new List<AsimovVersion>();

            var path = GetPackagesPath(packageInfo);
            CollectZipFiles(versions, path, 1);

            return versions.OrderByDescending(x => x.Timestamp).ToList();
        }

        private string GetPackagesPath(PackageInfo packageInfo)
        {
            return Path.Combine(Uri.LocalPath, packageInfo.SourceRelativePath ?? "");
        }

        public override AsimovVersion GetVersion(string versionId, PackageInfo packageInfo)
        {
            var fileInfo = GetFilePathForVersion(versionId);
            return GetVersionInfoFromFile(fileInfo);
        }

        public override string CopyAndExtractToTempFolder(string versionId, PackageInfo packageInfo, string tempFolder, string downloadFolder)
        {
            var fileInfo = GetFilePathForVersion(versionId);
            var localZipFileName = Path.Combine(downloadFolder, fileInfo.Name);

            if (!DirectoryUtil.Exists(localZipFileName))
                File.Copy(fileInfo.FullName, localZipFileName, true);

            Extract(localZipFileName, tempFolder, packageInfo.InternalPath);

            return Path.Combine(tempFolder, packageInfo.InternalPath);
        }


        private FileInfo GetFilePathForVersion(string versionId)
        {
            var versionPath = Path.Combine(Uri.LocalPath, versionId);
            if (!File.Exists(versionPath))
            {
                throw new FileNotFoundException("Could not find version file " + versionId);
            }

            return new FileInfo(versionPath);
        }

        private void CollectZipFiles(IList<AsimovVersion> versions, string directory, int level)
        {
            foreach (var filePath in Directory.EnumerateFiles(directory, "*.zip"))
            {
                var fileInfo = new FileInfo(filePath);
                var version = GetVersionInfoFromFile(fileInfo);
                if (version != null)
                    versions.Add(version);
            }

            if (level <= 2)
            {
                foreach (var subDir in Directory.EnumerateDirectories(directory))
                {
                    CollectZipFiles(versions, subDir, level + 1);
                }
            }
        }

        private AsimovVersion GetVersionInfoFromFile(FileInfo fileInfo)
        {
            var version = AsimovVersion.Parse(Pattern,fileInfo.Name,fileInfo.LastAccessTime);
            if (version != null)
            {
                version.Id = fileInfo.FullName.Replace(Uri.LocalPath, "");
                version.Id = version.Id.TrimStart(new[] { '\\' });
            }
            return version;
        }
    }
}