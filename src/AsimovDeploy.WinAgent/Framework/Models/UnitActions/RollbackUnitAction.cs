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
using System.Collections.Generic;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using StructureMap;
using log4net;

namespace AsimovDeploy.WinAgent.Framework.Models.UnitActions
{
    public class RollbackUnitAction : UnitAction
    {
        private static ILog Log = LogManager.GetLogger(typeof (RollbackUnitAction));

        public RollbackUnitAction()
        {
            base.Name = "Rollback";
        }

        public override AsimovTask GetTask(DeployUnit unit, AsimovUser user)
        {

            var previousversion = unit.GetDeployedVersions().Skip(1).FirstOrDefault(x => x.DeployFailed == false);
            if (previousversion == null)
            {
                Log.Warn("Could not find any previous version!");
                return null;
            }
            
            var config = ObjectFactory.GetInstance<IAsimovConfig>();

            var packageSource = config.GetPackageSourceFor(unit);
            var version = packageSource.GetVersion(previousversion.VersionId, unit.PackageInfo);
            return unit.GetDeployTask(version, new ParameterValues(previousversion.Parameters), user);

        }
    }
}