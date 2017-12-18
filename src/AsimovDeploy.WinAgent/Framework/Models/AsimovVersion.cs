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
using System.Text.RegularExpressions;

namespace AsimovDeploy.WinAgent.Framework.Models
{
    public class AsimovVersion
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Number { get; set; }
        public string Branch { get; set; }
        public string Commit { get; set; }

        public const string DefaultPattern =
            @"v(?<version>\d+\.\d+\.\d+\.\d+)-\[(?<branch>[\w\-]*)\]-\[(?<commit>\w*)\]";


        public static AsimovVersion Parse(string pattern, string versionString, DateTime lastModified)
        {
            var match = Regex.Match(versionString, pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            var version = new AsimovVersion
            {
                Id = versionString,
                Number = match.Groups["version"].Value,
                Branch = match.Groups["branch"].Value,
                Commit = match.Groups["commit"].Value,
                Timestamp = lastModified
            };
            return version;
        }
    }


}