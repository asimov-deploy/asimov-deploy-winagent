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
    public class TextActionParameterTests
    {
        private TextActionParameter _textParam;

        [OneTimeSetUp]
        public void Arrange()
        {
            _textParam = new TextActionParameter
            {
                Default = "testing value",
                Name = "tasks"
            };
        }

         [Test]
         public void can_get_descriptor()
         {
             var descriptor = _textParam.GetDescriptor();
             ((string) descriptor.name).ShouldBe("tasks");
             ((string) descriptor.type).ShouldBe("text");
             ((string) descriptor.@default).ShouldBe("testing value");
         }

        [Test]
        public void can_apply_to_powershell_script()
        {
            var script = new StringBuilder();
            _textParam.ApplyToPowershellScript(script, "some value");
            script.ToString().ShouldContain("$tasks = \"some value\"");
        }
    }
}