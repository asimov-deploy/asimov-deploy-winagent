using System;

namespace AsimovDeploy.WinAgentUpdater
{
    public class AsimovConfigUpdate
    {
        public string FilePath { get; set; }
        public int Version { get; set; }
        public string Bucket { get; set; }
        public string FileNameGS { get; set; }
        public string FileName { get; set; }        
    }

    public class AsimovVersion
    {
        public string FilePath { get; set; }
        public Version Version { get; set; }
        public string Bucket { get; set; }
        public string FileNameGS { get; set; }   
        public string FileName { get; set; }        
    }
}