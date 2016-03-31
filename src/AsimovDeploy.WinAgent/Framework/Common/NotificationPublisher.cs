using System.Collections.Generic;
using AsimovDeploy.WinAgent.Framework.Events;

namespace AsimovDeploy.WinAgent.Framework.Common
{
    public static class NotificationPublisher
    {
        private static readonly List<INotifier> Publishers = new List<INotifier>()
        {
            new NodeFront(), 
            new WebNotificationPublisher()
        };

        public static void PublishNotifications(AsimovEvent evt)
        {
            foreach (var publisher in Publishers)
                publisher.Notify(evt);
        }

    }
}