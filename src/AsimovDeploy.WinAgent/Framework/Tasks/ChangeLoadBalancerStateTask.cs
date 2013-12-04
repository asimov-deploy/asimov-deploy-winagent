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

using System.Threading;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Events;
using AsimovDeploy.WinAgent.Framework.LoadBalancers;
using AsimovDeploy.WinAgent.Web.Commands;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
	public class ChangeLoadBalancerStateTask : AsimovTask
	{
		private readonly ChangeLoadBalancerStateCommand _command;
		private readonly ILoadBalancerService loadBalancerService;
        private readonly NodeFront _nodefront = new NodeFront();
		public ChangeLoadBalancerStateTask(ChangeLoadBalancerStateCommand command, ILoadBalancerService loadBalancerService)
		{
			_command = command;
			this.loadBalancerService = loadBalancerService;
		}

		protected override string InfoString()
		{
			return string.Format("Server name: {0}, action={1}", Config.LoadBalancerServerId, _command.action);
		}

		protected override void Execute()
		{
			if (_command.action == "enable")
			{
				loadBalancerService.EnableServer();
				Thread.Sleep(1000);
                _nodefront.Notify(new LoadBalancerStateChanged(loadBalancerService.GetCurrentState()));
			}
			else if (_command.action == "disable")
			{
				loadBalancerService.DisableServer();
				Thread.Sleep(1000);
                _nodefront.Notify(new LoadBalancerStateChanged(loadBalancerService.GetCurrentState()));
			}
			else
			{
				Log.Error("Invalid load balancer action: " + _command.action);
			}
		}
	}
}