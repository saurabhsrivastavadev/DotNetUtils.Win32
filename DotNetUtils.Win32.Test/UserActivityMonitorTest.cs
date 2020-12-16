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
            bool exceptionRaised = false;

            try
            {
                exceptionRaised = false;
                var uam = new UserActivityMonitor("");
            }
            catch (Exception) { exceptionRaised = true; }
            Assert.IsTrue(exceptionRaised, "Empty app name should not be accepted.");

            try
            {
                exceptionRaised = false;
                var uam = new UserActivityMonitor("   ");
            }
            catch (Exception) { exceptionRaised = true; }
            Assert.IsTrue(exceptionRaised, "Whitespace app name should not be accepted.");

            try
            {
                exceptionRaised = false;
                var uam = new UserActivityMonitor(null);
            }
            catch (Exception) { exceptionRaised = true; }
            Assert.IsTrue(exceptionRaised, "null app name should not be accepted.");
        }

        [TestMethod]
        public void TestClearAllUserActivityStats()
        {
            var uam = new UserActivityMonitor("DotNetUtils.Win32.Test");
            uam.ClearAllUserActivityStats();
        }
    }
}
