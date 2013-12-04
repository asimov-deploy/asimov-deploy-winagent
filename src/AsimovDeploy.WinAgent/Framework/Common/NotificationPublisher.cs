using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using AsimovDeploy.WinAgent.Framework.Events;
using AsimovDeploy.WinAgent.Framework.Models;
using Newtonsoft.Json;
using StructureMap;

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
            {
                publisher.Notify(evt);
            }

        }

    }
    public class WebNotificationPublisher:INotifier
    {
        private static readonly string _webNotificationUrl;

        static WebNotificationPublisher()
        {
            var config = ObjectFactory.GetInstance<IAsimovConfig>();
            _webNotificationUrl = config.WebNotificationUrl;
        }

        public void Notify(AsimovEvent evt)
        {
            try
            {
                if (String.IsNullOrEmpty(_webNotificationUrl) ) return;

                var url = new Uri(_webNotificationUrl);

                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ServicePoint.Expect100Continue = false;
                request.Timeout = 30000;

                using (var requestStream = request.GetRequestStream())
                {
                    using (var writer = new StreamWriter(requestStream))
                    {
                        var serializer = new JsonSerializer();
                        serializer.NullValueHandling = NullValueHandling.Ignore;
                        serializer.Serialize(writer, evt);
                    }
                }

                using (var resp = request.GetResponse())
                {
                    resp.Close();
                }

            }
            catch (Exception ex)
            {
                Console.Write("Error sending event from webnotification publisher");
            }
        }
    }
}