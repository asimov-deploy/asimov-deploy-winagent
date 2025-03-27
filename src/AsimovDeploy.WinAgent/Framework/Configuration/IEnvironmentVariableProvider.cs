using System;

namespace AsimovDeploy.WinAgent.Framework.Configuration
{
    public interface IEnvironmentVariableProvider
    {
        string GetEnvironmentVariable(string name);
    }

    public class DefaultEnvironmentVariableProvider : IEnvironmentVariableProvider
    {
        public string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }
    }
} 