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
using System.IO;
using System.Security.Cryptography;

namespace AsimovDeploy.WinAgent.Framework.Common
{
    public static class DirectoryUtil
    {
        public static void Clean(string directory)
        {
            if (!Exists(directory))
                return;

            var dir = new DirectoryInfo(directory);
            foreach (FileInfo file in dir.GetFiles())
                file.Delete();

            foreach (DirectoryInfo subDirectory in dir.GetDirectories()) subDirectory.Delete(true);
        }

        public static void CleanOldFiles(string directory, TimeSpan maxAge)
        {
            if (!Exists(directory))
                return;

            var dir = new DirectoryInfo(directory);

            var timeOfMaxAge = DateTime.Now - maxAge;
            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.CreationTime < timeOfMaxAge)
                    file.Delete();
            }
        }

        public static void CopyDirectory(string sourcePath, string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }

            foreach (string folder in Directory.GetDirectories(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(folder));
                CopyDirectory(folder, dest);
            }
        }

        public static bool Exists(string path)
        {
            return Directory.Exists(path) || File.Exists(path);
        }

        public static string Md5(string localFileName)
        {
            using (var fileStream = File.OpenRead(localFileName))
            {
                var md5 = new MD5CryptoServiceProvider();
                var md5String = Convert.ToBase64String(md5.ComputeHash(fileStream));
                return md5String;
            }
        }
    }
}