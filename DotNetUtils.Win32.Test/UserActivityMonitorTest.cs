using System.Threading;
using DotNetUtils.Win32.UserActivity;
using DotNetUtils.Win32.UserActivity.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetUtils.Win32.Test
{
    [TestClass]
    public class UserActivityMonitorTest
    {
        [TestMethod]
        public void TestClearAllUserActivityStats()
        {
            var uam = new UserActivityMonitor();
            uam.ClearAllUserActivityStats();
        }
    }
}
