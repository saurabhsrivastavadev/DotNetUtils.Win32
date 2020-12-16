using System;
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
        public void TestFactoryAppNameValidation()
        {
            try
            {
                var uam = new UserActivityMonitor("");
                Assert.Fail("Empty app name should not be accepted.");
            }
            catch (Exception) { }

            try
            {
                var uam = new UserActivityMonitor("   ");
                Assert.Fail("Whitespace app name should not be accepted.");
            }
            catch (Exception) { }

            try
            {
                var uam = new UserActivityMonitor(null);
                Assert.Fail("null app name should not be accepted.");
            }
            catch (Exception) { }
        }

        [TestMethod]
        public void TestClearAllUserActivityStats()
        {
            var uam = new UserActivityMonitor("DotNetUtils.Win32.Test");
            uam.ClearAllUserActivityStats();
        }
    }
}
