// /*******************************************************************************
// * Copyright (C) 2012 eBay Inc.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// *   http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// ******************************************************************************/
using System.Text.RegularExpressions;
using AsimovDeploy.WinAgent.Framework.Models.PackageSources;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.Tests.Packagesource
{
    public class FileSystemPackageSourceRegexTest
    {
        [Test]
        public void Branches_can_contain_dash()
        {
            var p = new FileSystemPackageSource().Pattern;
            var match = Regex.Match("Tradera-v14.5.0.41-[produ-ct_ion]-[f3b4995].prod", p);

            match.Success.ShouldBe(true);
        }
    }
}