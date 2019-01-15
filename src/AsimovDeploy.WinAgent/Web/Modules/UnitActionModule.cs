using System.Linq;
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
                if (action == null)
                {
                    return Response.AsJson(new
                    {
                        OK = false, 
                        Message=$"No action found with name {command.actionName}.", 
                        AvailableActions=deployUnit.Actions.Select(x=>x.Name)
                    }, HttpStatusCode.BadRequest);
                }
                var asimovUser = new AsimovUser() { UserId = command.userId, UserName = command.userName };

                var task = action.GetTask(deployUnit, asimovUser, command.correlationId);

                if (task != null)
                    taskExecutor.AddTask(task);

                return Response.AsJson(new { OK = true });
            };
        }
    }
}