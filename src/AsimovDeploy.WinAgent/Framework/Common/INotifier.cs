using AsimovDeploy.WinAgent.Framework.Events;

namespace AsimovDeploy.WinAgent.Framework.Common
{
    public interface INotifier
    {
        void  Notify(AsimovEvent data);
    }
}