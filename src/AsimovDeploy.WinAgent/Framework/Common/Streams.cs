using System;
using System.IO;
using System.Text;
using System.Threading;

namespace AsimovDeploy.WinAgent.Framework.Common
{
    public static class Streams
    {
        public static void RedirectOutput(this TextReader input, Action<string> logAction)
        {
            new Thread(a =>
            {
                var buffer = new char[1];
                var str = new StringBuilder();
                while (input.Read(buffer, 0, 1) > 0)
                {
                    str.Append(buffer[0]);
                    if (buffer[0] == '\n')
                    {
                        logAction(str.ToString());
                        str.Clear();
                    }
                }
            }).Start();
        }
    }
}
