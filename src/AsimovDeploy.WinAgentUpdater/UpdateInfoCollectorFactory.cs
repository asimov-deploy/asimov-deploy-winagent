using System;
using System.Configuration;
using System.Text.RegularExpressions;

namespace AsimovDeploy.WinAgentUpdater
{
    public class UpdateInfoCollectorFactory
    {
        public static IUpdateInfoCollector GetCollector()
        {
            var watchFolder = ConfigurationManager.AppSettings["Asimov.WatchFolder"];
            var port = Int32.Parse(ConfigurationManager.AppSettings["Asimov.WebPort"]);

            var googleStoragePattern = new Regex(@"(gs:)//", RegexOptions.IgnoreCase);
            var useGoogleStorage = googleStoragePattern.Match(watchFolder);

            IUpdateInfoCollector collector = useGoogleStorage.Success
                ? GetGoogleStorageCollector(watchFolder, port)
                : GetFileUpdateInfoCollector(watchFolder, port);
            return collector;
        }

        private static UpdateInfoCollector GetFileUpdateInfoCollector(string watchFolder, int port)
        {
            return new UpdateInfoCollector(watchFolder, port);
        }

        private static IUpdateInfoCollector GetGoogleStorageCollector(string watchFolder, int port)
        {
            return new GoogleStorageUpdateInfoCollector(watchFolder, port);
        }
    }
}