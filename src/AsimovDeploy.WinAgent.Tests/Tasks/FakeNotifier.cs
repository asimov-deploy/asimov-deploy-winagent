using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Events;

namespace AsimovDeploy.WinAgent.Tests.Tasks
{
    public class FakeNotifier : INotifier
    {
        public bool WasNotified = false;

        public AsimovEvent LastEvent { get; set; }
        
        public void Notify(AsimovEvent data)
        {
            LastEvent = data;
            WasNotified = true;
        }
    }
}