using System;
using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using Ionic.Zip;
using log4net;

namespace AsimovDeploy.WinAgentUpdater
{
    public class Updater
    {
        private Timer _timer;

        private static ILog _log = LogManager.GetLogger(typeof(Updater));
        private string _installDir;
        private const int interval = 4000;

        private IUpdateInfoCollector _collector;
        
        public void Start()
        {
            _collector = UpdateInfoCollectorFactory.GetCollector();
            _installDir = ConfigurationManager.AppSettings["Asimov.InstallDir"];

            _timer = new Timer(TimerTick, null, 0, interval);
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void TimerTick(object state)
        {

            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            _log.Info("Looking for new version");

            try
            {
                UpdateInfo updateInfo = _collector.Collect();
                
                _log.InfoFormat(updateInfo.ToString());

                using (var service = new ServiceController("AsimovDeploy.WinAgent"))
                {
                    if (!updateInfo.NeedsAnyUpdate())
                        return;
                    
                    StopService(service);
                    
                    if (updateInfo.NewBuildFound())
                    {
                        UpdateWinAgentWithNewBuild(updateInfo.LastBuild);
                        if (updateInfo.HasLastConfig)
                        {
                            UpdateWinAgentConfig(updateInfo.LastConfig);
                        }
                    }

                    if (updateInfo.NewConfigFound())
                    {
                        UpdateWinAgentConfig(updateInfo.LastConfig);
                    }

                    StartService(service);                    
                }
            }
            catch(Exception ex)
            {
                _log.Error("Failed to check for upgrade", ex);
            }
            finally
            {
                _timer.Change(interval, interval);
            }
        }

        private void UpdateWinAgentConfig(AsimovConfigUpdate lastConfig)
        {
            _log.Info("Updating config to version " + lastConfig.Version);

            var configDir = Path.Combine(_installDir, "ConfigFiles");

            CleanFolder(configDir);
            using (var packageStream = lastConfig.FileSource.GetStream())
            {
                CopyNewBuildToInstallDir(configDir, packageStream);
            }
        }

        private void UpdateWinAgentWithNewBuild(AsimovVersion lastBuild)
        {
            _log.InfoFormat("Installing new build {0}", lastBuild.Version);
            CleanFolder(_installDir);

            using (var packageStream = lastBuild.FileSource.GetStream())
            {
                CopyNewBuildToInstallDir(_installDir, packageStream);
            }
        }

        private static void StopService(ServiceController serviceController)
        {
            if (serviceController.Status == ServiceControllerStatus.Running)
            {
                _log.Info("Stopping AsimovDeploy...");
                serviceController.Stop();
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                _log.Info("AsimovDeploy stopped");
            }
            else
            {
                _log.Info("AsimovDeploy Service was not running, trying to update and start it");
            }
        }

        private void StartService(ServiceController serviceController)
        {
            _log.Info("Starting service...");
            serviceController.Start();
            serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
            
            _log.Info("Service  started");
        }

        private void CopyNewBuildToInstallDir(string installDir, Stream package)
        {
            using (var zipFile = ZipFile.Read(package))
            {
                zipFile.ExtractAll(installDir);
            }
        }

        private void CleanFolder(string destinationFolder)
        {
            if (destinationFolder.Contains("Asimov") == false)
            {
                throw new Exception("Asimov install dir does not contain asimov, will abort upgrade");
            }

            var dir = new DirectoryInfo(destinationFolder);
            foreach (FileInfo file in dir.GetFiles())
            {
                if (!file.Extension.Contains("log"))
                    file.Delete();
            }

            foreach (DirectoryInfo subDirectory in dir.GetDirectories()) subDirectory.Delete(true);
        }
    }
}