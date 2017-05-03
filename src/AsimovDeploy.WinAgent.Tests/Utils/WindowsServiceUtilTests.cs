using AsimovDeploy.WinAgent.Framework.Common;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.Tests.Utils
{
    [TestFixture]
    public class WindowsServiceUtilTests
    {
        [Test]
        public void should_return_service_executable_for_path_starting_with_double_quote()
        {
            var serviceExe = WindowsServiceUtil.GetServiceExecutable(@"""C:\temp\service.exe"" -arg value");
            serviceExe.ShouldBe(@"C:\temp\service.exe");
        }

        [Test]
        public void should_return_service_executable_for_path_not_starting_with_double_quote()
        {
            var serviceExe = WindowsServiceUtil.GetServiceExecutable(@"C:\temp\service.exe -arg value");
            serviceExe.ShouldBe(@"C:\temp\service.exe");
        }
    }
}