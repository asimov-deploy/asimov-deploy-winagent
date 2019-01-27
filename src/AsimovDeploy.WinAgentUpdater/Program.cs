using Topshelf;
using log4net;

namespace AsimovDeploy.WinAgentUpdater
{
    class Program
    {
        public static ILog _log = LogManager.GetLogger(typeof(Program));

        private const string ServiceName = "AsimovDeploy.WinAgentUpdater";
        
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var host = HostFactory.New(x =>
            {
                x.SetServiceName(ServiceName);

                x.Service<Updater>(s =>
                {
                    s.ConstructUsing(name => new Updater());

                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();

                x.SetDisplayName(ServiceName);
                x.SetDescription(ServiceName);
                x.SetServiceName(ServiceName);
            });

            host.Run();

            
        }
    }
}
