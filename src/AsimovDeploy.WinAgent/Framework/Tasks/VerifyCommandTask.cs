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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Events;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using Ionic.Zip;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
    public class EventArgsForCommand : EventArgs
    {
        public string OutputData { get; internal set; }
    }
    public class VerifyCommandTask : AsimovTask
    {
        private readonly WebSiteDeployUnit deployUnit;
        private readonly string zipPath;
        private readonly string command;
        private readonly string correlationId;
        protected Report report;
        public INotifier Nodefront { get; set; } = new NodeFront();

        public VerifyCommandTask(WebSiteDeployUnit webSiteDeployUnit, string zipPath, string command, string correlationId)
        {
            deployUnit = webSiteDeployUnit;
            this.zipPath = zipPath;
            this.command = command;
            this.correlationId = correlationId;
        }


        protected override void Execute()
        {
            CleanTempFolderAndExtractVerifyPackage();

            using (var p = new Process())
            {
                Nodefront.Notify(new VerifyProgressEvent(deployUnit.Name) { started = true });

                // Redirect the output stream of the child process.
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.WorkingDirectory = Config.TempFolder;

                var commandParts = GetCommandParts();

                p.StartInfo.FileName = Path.Combine(Config.TempFolder, commandParts[0]);
                p.StartInfo.CreateNoWindow = true;

                p.StartInfo.Arguments = string.Join(" ", commandParts, 1, commandParts.Length - 1);

                p.Start();

                ListenToStream(p.StandardOutput, ParseVerifyCommandOutput, () => Log.Debug("Verify command output ended"));
                ListenToStream(p.StandardError, line => Log.Error(line), () => Log.Debug("Verify command error output ended"));

                p.WaitForExit((int)TimeSpan.FromMinutes(10).TotalMilliseconds);

                if (!p.HasExited)
                    p.Kill();

                Nodefront.Notify(new VerifyProgressEvent(deployUnit.Name) { completed = true, report = report });
            }
        }

        private void CleanTempFolderAndExtractVerifyPackage()
        {
            DirectoryUtil.Clean(Config.TempFolder);

            var webAppInfo = deployUnit.GetWebServer().GetInfo();

            var zipPath = Path.Combine(webAppInfo.PhysicalPath, this.zipPath);

            if (Directory.Exists(zipPath))
            {
                DirectoryCopy(zipPath, Config.TempFolder);
            }
            else
            {
                using (var zipFile = ZipFile.Read(zipPath))
                {
                    zipFile.ExtractAll(Config.TempFolder);
                }
            }
        }

        private string[] GetCommandParts()
        {
            var siteUrl = deployUnit.SiteUrl.Replace("localhost", HostNameUtil.GetFullHostName());
            var verifyCommand = command.Replace("%SITE_URL%", siteUrl);
            var commandParts = verifyCommand.Split(new[] { ' ' });
            return commandParts;
        }

        private void ListenToStream(StreamReader input, Action<string> action, Action done)
        {
            new Thread(a =>
            {
                var buffer = new char[1];
                var str = new StringBuilder();
                while (input.Read(buffer, 0, 1) > 0)
                {
                    str.Append(buffer[0]);
                    if (buffer[0] == '\n')
                    {
                        var line = str.ToString();
                        action(line);
                        str.Clear();
                    }
                };

                done();
            }).Start();
        }


        public void ParseVerifyCommandOutput(string line)
        {
            Log.Debug(line);

            if (line.StartsWith("##asimov-deploy"))
            {
                HandleAsimovMessage(line);
            }
        }

        public class Report
        {
            public string title { get; set; }
            public string url { get; set; }
        }

        private void HandleAsimovMessage(string line)
        {
            var keys = ConsoleOutputParseUtil.ParseKeyValueString(line);

            if (keys.ContainsKey("image"))
            {
                Nodefront.Notify(new VerifyProgressEvent(deployUnit.Name)
                {
                    image = new Report { title = keys["title"], url = GetUrlForFileInTempReportsFolder(keys["image"]) }
                });
            }

            if (keys.ContainsKey("test"))
            {
                Nodefront.Notify(new VerifyProgressEvent(deployUnit.Name)
                {
                    test = new { pass = keys["pass"] == "true", message = keys["test"] }
                });
            }

            if (keys.ContainsKey("report"))
            {
                report = new Report { title = keys["title"], url = GetUrlForFileInTempReportsFolder(keys["report"]) };
            }
        }

        private string GetUrlForFileInTempReportsFolder(string file)
        {
            var uri = new Uri(file, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                return file;
            }
            return new Uri(Config.WebControlUrl, "temp-reports/" + file).ToString();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    $"Source directory does not exist or could not be found: {sourceDirName}");
            }

            var dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            foreach (var subDir in dirs)
            {
                DirectoryCopy(subDir.FullName, Path.Combine(destDirName, subDir.Name));
            }
        }
    }
}
