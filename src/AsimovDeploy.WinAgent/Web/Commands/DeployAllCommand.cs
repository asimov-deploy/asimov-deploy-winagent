using System.Collections.Generic;

namespace AsimovDeploy.WinAgent.Web.Commands
{
    public class DeployAllCommand : AsimovCommand
    {
        public string correlationId { get; set; }
        public string preferredBranch { get; set; }
        public string userId { get; set; }
        public string userName { get; set; }
    }
}