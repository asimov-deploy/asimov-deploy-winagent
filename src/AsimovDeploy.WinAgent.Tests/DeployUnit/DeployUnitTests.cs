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
using AsimovDeploy.WinAgent.Framework.Models.Units;
using NUnit.Framework;

namespace AsimovDeploy.WinAgent.Tests.DeployUnit
{
    [TestFixture]

    public class DeployUnitTests
    {
        [Test]
        public void test_only_on_agents_no_list()
        {
            // Arrange
            var unit = new WindowsServiceDeployUnit ();

            // Act & Assert
            Assert.That(unit.IsValidForAgent("test_machine"), Is.True);

        }
        [Test]
        public void test_only_on_agents_not_in_list()
        {
            // Arrange
            var unit = new WindowsServiceDeployUnit { OnlyOnAgents = new[] { "test_machine-02" } };

            // Act & Assert
            Assert.That(unit.IsValidForAgent("test_machine"), Is.False);

        }
        [Test]
        public void test_only_on_agents_when_in_list()
        {
            // Arrange
            var unit = new WindowsServiceDeployUnit { OnlyOnAgents = new[] { "test_machine-02","test_machine-01" } };

            // Act & Assert
            Assert.That(unit.IsValidForAgent("test_machine-01"), Is.True);

        }

        [Test]
        public void test_only_on_agents_when_wildcardmatch()
        {
            // Arrange
            var unit = new WindowsServiceDeployUnit { OnlyOnAgents = new[] { "test_machine-*" } };

            // Act & Assert
            Assert.That(unit.IsValidForAgent("test_machine-01"), Is.True);

        }
        [Test]
        public void test_only_on_agents_when_direct_and_wildcardmatch()
        {
            // Arrange
            var unit = new WindowsServiceDeployUnit { OnlyOnAgents = new[] { "test_machine-01", "test_machine-*" } };

            // Act & Assert
            Assert.That(unit.IsValidForAgent("test_machine-01"), Is.True);

        }
    }
}