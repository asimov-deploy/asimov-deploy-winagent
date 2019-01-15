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

using System.Collections.Generic;
using System.Linq;
using AsimovDeploy.WinAgent.Framework.Configuration;
using Newtonsoft.Json;

namespace AsimovDeploy.WinAgent.Framework.Models.UnitActions
{
    [JsonConverter(typeof(AsimovListJsonConverter))]
    [AsimovListType("VerifyUrls", typeof(VerifyUrlsUnitAction))]
    [AsimovListType("VerifyCommand", typeof(VerifyCommandUnitAction))]
    [AsimovListType("Command", typeof(CommandUnitAction))]
    [AsimovListType("PowerShell", typeof(PowerShellUnitAction))]
    public class UnitActionList : List<UnitAction>
    {
        public UnitAction this[string name] => this.SingleOrDefault(x => x.Name == name);


        /// <summary>
        /// Adds an UnitAction if there are no actions of the same type in the collection. Does nothing otherwise.
        /// </summary>
        /// <returns>True if the action was added</returns>
        public bool AddIfFirstOfType<T>(T action) where T : UnitAction
        {
            if (this.OfType<T>().Any())
                return false;
            Add(action);
            return true;
        }
    }
}
