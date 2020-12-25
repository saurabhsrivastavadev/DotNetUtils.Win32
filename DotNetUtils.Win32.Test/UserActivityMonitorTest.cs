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
        private const string TestAppName = "DotNetUtils.Win32.Test";

        [TestMethod]
        public void TestFactoryAppNameValidation()
        {
            bool exceptionRaised;

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
            var uam = new UserActivityMonitor(TestAppName);

            uam.ClearAllUserActivityStats();

            TimeSpan fetchUserActivityForTimespan = TimeSpan.FromDays(10);
            var stats = uam.GetUserActivityStats(
                DateTime.Now.Subtract(fetchUserActivityForTimespan), DateTime.Now);

            Assert.AreEqual(stats.TotalActiveTime.TotalSeconds, 0);
            Assert.AreEqual(stats.TotalInactiveTime.TotalSeconds, 0);
            Assert.AreEqual(stats.TotalUnmonitoredTime.CompareTo(fetchUserActivityForTimespan), 0);
        }

        [TestMethod]
        public void TestMonitorAndTalyStats()
        {
            var uam = new UserActivityMonitor(TestAppName);

            int monitorForSeconds = 30;

            uam.ClearAllUserActivityStats();

            uam.StartMonitoring();
            Thread.Sleep(monitorForSeconds * 1000);
            uam.StopMonitoring();

            TimeSpan fetchUserActivityForTimespan = TimeSpan.FromMinutes(10);
            var stats = uam.GetUserActivityStats(
                DateTime.Now.Subtract(fetchUserActivityForTimespan), DateTime.Now);

            TimeSpan totalMonitoringTime =
                stats.TotalActiveTime + stats.TotalInactiveTime + stats.TotalUnmonitoredTime;

            Assert.IsTrue(monitorForSeconds - 3 < totalMonitoringTime.TotalSeconds &&
                totalMonitoringTime.TotalSeconds < monitorForSeconds + 3);
        }

        [TestMethod]
        public void TestMonitor()
        {
            var uam = new UserActivityMonitor(TestAppName);

            uam.StartMonitoring();

            Thread.Sleep(180000);

            uam.StopMonitoring();
        }
    }
}
