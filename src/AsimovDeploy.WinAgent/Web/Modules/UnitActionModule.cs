using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Web.Commands;
using Nancy;
using Nancy.ModelBinding;

namespace AsimovDeploy.WinAgent.Web.Modules
{
	public class UnitActionModule : NancyModule
	{
		public UnitActionModule(ITaskExecutor taskExecutor, IAsimovConfig config)
		{
			Post["/action"] = _ =>
			{
				var command = this.Bind<UnitActionCommand>();
				var deployUnit = config.GetUnitByName(command.unitName);
				var action = deployUnit.Actions[command.actionName];
			    var asimovUser = new AsimovUser() {UserId = command.userId, UserName = command.userName};

				var task = action.GetTask(deployUnit,asimovUser);

                if(task != null)
				    taskExecutor.AddTask(task);

				return Response.AsJson(new { OK = true });
			};
		}
	}
}