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

using System.Text;
using AsimovDeploy.WinAgent.Framework.Models;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.Tests.ActionParameters
{
    [TestFixture]
    public class PasswordActionParameterWithoutPasswordSetTests
    {
        private PasswordActionParameter _passwordParam;

        [TestFixtureSetUp]
        public void Arrange()
        {
            _passwordParam = new PasswordActionParameter
            {
                Name = "pwd",
                Default = "DefaultPassword"
            };
        }

        [Test]
        public void can_get_descriptor()
        {
            var descriptor = _passwordParam.GetDescriptor();
            ((string)descriptor.name).ShouldBe("pwd");
            ((string)descriptor.type).ShouldBe("password");
            ((string)descriptor.@default).ShouldBe("DefaultPassword");
        }

        [Test]
        public void can_apply_to_powershell_script()
        {
            var script = new StringBuilder();
            _passwordParam.ApplyToPowershellScript(script, "Password!");
            script.ToString().ShouldContain("$pwd = \"Password!\"");
        }
    }
}