using System;
using System.IO;
using Google.Cloud.Storage.V1;

namespace AsimovDeploy.WinAgentUpdater
{
    public class AsimovVersion
    {
        public Version Version { get; set; }
        public IAsimovFileSource FileSource { get; set; }
    }
    public class AsimovConfigUpdate
    {
        public int Version { get; set; }
        public IAsimovFileSource FileSource { get; set; }
    }

    public interface IAsimovFileSource
    {
        string GetFilePath();
    }

    public class FileSystemFileSource : IAsimovFileSource
    {
        private readonly string _path;

        public FileSystemFileSource(string path)
        {
            _path = path;
        }

        public string GetFilePath()
        {
            return _path;
        }
    }

    public class GcpAsimovFileSource : IAsimovFileSource
    {
        private readonly Google.Apis.Storage.v1.Data.Object Object;
        private readonly StorageClient Client;


        public GcpAsimovFileSource(Google.Apis.Storage.v1.Data.Object o, StorageClient client)
        {
            Object = o;
            Client = client;
        }

        public string GetFilePath()
        {            
            var tempFile = Path.GetTempFileName();

            using (var fileStrem = File.OpenWrite(tempFile))
            {
                Client.DownloadObject(Object, fileStrem);
            }
            return tempFile;
        }
    }
}