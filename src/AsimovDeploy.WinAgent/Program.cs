using System;
using AsimovDeploy.WinAgent.Service;
using Topshelf;
using log4net;

namespace AsimovDeploy.WinAgent
{
    class Program
    {
        public static ILog _log = LogManager.GetLogger(typeof (Program));

        private const string ServiceName = "AsimovDeploy.WinAgent";

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            
            var host = HostFactory.New(x =>
            {
                x.SetServiceName(ServiceName);

                x.Service<IAsimovDeployService>(s =>
                {
                    s.ConstructUsing(name => new AsimovDeployService());

                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                
                x.RunAsLocalSystem();
                x.SetDisplayName(ServiceName);
                x.SetDescription(ServiceName);
                x.SetServiceName(ServiceName);
            });

            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            
            host.Run();
        }

        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Error("Unhandled exception", (Exception)e.ExceptionObject);
        }
    }
}
