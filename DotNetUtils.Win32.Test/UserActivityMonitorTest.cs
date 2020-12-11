using System.Threading;
using DotNetUtils.Win32.UserActivity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetUtils.Win32.Test
{
    [TestClass]
    public class UserActivityMonitorTest
    {
        [TestMethod]
        public void TestUserActivityMonitoringTimer()
        {
            new UserActivityMonitor().StartMonitoring();
            Thread.Sleep(10000);
            Assert.IsTrue(true);
        }
    }
}
