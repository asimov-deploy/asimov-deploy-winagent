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
        Stream GetStream();
    }

    public class FileSystemFileSource : IAsimovFileSource
    {
        private readonly string _path;

        public FileSystemFileSource(string path)
        {
            _path = path;
        }

        public Stream GetStream()
        {
            return File.OpenRead(_path);
        }
    }

    public class GoogleStorageFileSource : IAsimovFileSource
    {
        private readonly Google.Apis.Storage.v1.Data.Object Object;
        private readonly StorageClient Client;

        public GoogleStorageFileSource(Google.Apis.Storage.v1.Data.Object o, StorageClient client)
        {
            Object = o;
            Client = client;
        }

        public Stream GetStream()
        {
            var outputStream = new MemoryStream();
            Client.DownloadObject(Object, outputStream);
            outputStream.Position = 0;
            return outputStream;
        }
    }
}