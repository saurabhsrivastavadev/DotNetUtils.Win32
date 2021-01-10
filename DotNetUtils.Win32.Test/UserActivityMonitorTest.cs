using System;
using System.Collections.Generic;
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
        public void TestEmptyAppNameNotAllowed()
        {
            bool exceptionRaised;
            UserActivityMonitor.ResetInstance();

            try
            {
                exceptionRaised = false;
                var uam = UserActivityMonitor.GetInstance("");
            }
            catch (Exception) { exceptionRaised = true; }
            Assert.IsTrue(exceptionRaised, "Empty app name should not be accepted.");

            try
            {
                exceptionRaised = false;
                var uam = UserActivityMonitor.GetInstance("   ");
            }
            catch (Exception) { exceptionRaised = true; }
            Assert.IsTrue(exceptionRaised, "Whitespace app name should not be accepted.");

            try
            {
                exceptionRaised = false;
                var uam = UserActivityMonitor.GetInstance(null);
            }
            catch (Exception) { exceptionRaised = true; }
            Assert.IsTrue(exceptionRaised, "null app name should not be accepted.");

            UserActivityMonitor.ResetInstance();
        }

        [TestMethod]
        public void TestAppNameUpdateNotAllowed()
        {
            bool exceptionRaised;
            bool isFirstInstance = UserActivityMonitor.Instance == null;
            UserActivityMonitor uamFirst = null, uam = null;

            UserActivityMonitor.ResetInstance();

            try
            {
                exceptionRaised = false;
                uamFirst = UserActivityMonitor.GetInstance("DotNetUtils.Win32.Test.Temp.Delete");
            }
            catch (Exception) { exceptionRaised = true; }
            if (isFirstInstance)
            {
                Assert.IsFalse(exceptionRaised, "No exception expected for first instance with valid app name.");
            }
            else
            {
                Assert.IsTrue(exceptionRaised, "Exception for second instance request with different app name.");
            }

            try
            {
                exceptionRaised = false;
                uam = UserActivityMonitor.GetInstance("DotNetUtils.Win32.Test.Temp.Delete");
            }
            catch (Exception) { exceptionRaised = true; }
            Assert.IsFalse(exceptionRaised, "No exception expected for GetInstance with same app name.");
            Assert.IsTrue(uam == uamFirst, "Same instance expected with same AppName.");

            try
            {
                exceptionRaised = false;
                uam = UserActivityMonitor.GetInstance("DotNetUtils.Win32.Test.Temp.Delete.Second");
            }
            catch (Exception) { exceptionRaised = true; }
            Assert.IsTrue(exceptionRaised, "Exception expected for second GetInstance with same app name.");

            UserActivityMonitor.ResetInstance();
        }

        [TestMethod]
        public void TestClearAllUserActivityStats()
        {
            var uam = UserActivityMonitor.GetInstance(TestAppName);

            uam.ClearAllUserActivityStats();

            TimeSpan fetchUserActivityForTimespan = TimeSpan.FromDays(10);

            var stats = uam.GetUserActivityStats(
                DateTime.Now.Subtract(fetchUserActivityForTimespan), DateTime.Now);

            TimeSpan totalStatsDuration = stats.TotalActiveTime + stats.TotalInactiveTime +
                                            stats.TotalUnmonitoredTime;
            Assert.AreEqual(totalStatsDuration.TotalMilliseconds, 0);
        }

        [TestMethod]
        [DataRow(30)]
        public void TestClearAndGetStats(int monitorForSeconds)
        {
            var uam = UserActivityMonitor.GetInstance(TestAppName);

            Factory.MonitoringInterval = TimeSpan.FromSeconds(5);
            uam.UserInactivityThreshold = TimeSpan.FromSeconds(10);

            uam.ClearAllUserActivityStats();

            uam.StartMonitoring();
            Thread.Sleep(monitorForSeconds * 1000);
            uam.StopMonitoring();

            Thread.Sleep((int)Factory.MonitoringInterval.TotalMilliseconds * 3);

            TimeSpan fetchUserActivityForTimespan = TimeSpan.FromMinutes(10);
            var stats = uam.GetUserActivityStats(
                DateTime.Now.Subtract(fetchUserActivityForTimespan), DateTime.Now);

            double statsDelta = GetStatsActiveInactiveDurationDelta(
                                    TimeSpan.FromSeconds(monitorForSeconds), stats);
            Assert.IsTrue(statsDelta < 1, $"stats delta {statsDelta} exceeds limit (1 second)");
        }

        [TestMethod]
        public void TestMonitor()
        {
            var uam = UserActivityMonitor.GetInstance(TestAppName);

            uam.StartMonitoring();

            Thread.Sleep(120000);

            uam.StopMonitoring();
        }

        [TestMethod]
        [DataRow(30, true)]
        [DataRow(30, false)]
        public void TestGetUserActivityStats(int statsDurationSec, bool startMonitoring)
        {
            Factory.MonitoringInterval = TimeSpan.FromSeconds(5);
            TimeSpan statsDuration = TimeSpan.FromSeconds(statsDurationSec);

            var uam = UserActivityMonitor.GetInstance(TestAppName);
            uam.UserInactivityThreshold = TimeSpan.FromSeconds(10);

            if (startMonitoring)
            {
                uam.StartMonitoring();
                Thread.Sleep((int)statsDuration.TotalMilliseconds);
                uam.StopMonitoring();
            }

            DateTime statsFrom = DateTime.Now - statsDuration;
            DateTime statsTo = DateTime.Now;

            UserActivityStats stats = uam.GetUserActivityStats(statsFrom, statsTo);

            Assert.IsTrue(GetStatsTotalDurationDelta(statsDuration, stats) < 1);
        }

        [TestMethod]
        [DataRow(12, 0)]
        [DataRow(120, 0)]
        [DataRow(120000, 0)]
        [DataRow(120000, 100)]
        public void TestNoConsecutiveRowsWithSameActivityState(
                            int statsDurationSec, int monitoringCycles)
        {
            TimeSpan statsDuration = TimeSpan.FromSeconds(statsDurationSec);
            DateTime statsFrom = DateTime.Now - statsDuration;
            DateTime statsTo = DateTime.Now;

            var uam = UserActivityMonitor.GetInstance(TestAppName);

            for (int i = 0; i < monitoringCycles; i++)
            {
                uam.StartMonitoring();
                uam.StopMonitoring();
                uam.GetUserActivityStats(
                    DateTime.Now.Subtract(TimeSpan.FromMinutes(30)), DateTime.Now);
            }

            var stats = uam.GetUserActivityStats(statsFrom, statsTo);

            if (stats.CompleteSessionList.Count > 0)
            {
                UserActivityState lastState = stats.CompleteSessionList[0].ActivityState;
                foreach (var stat in stats.CompleteSessionList.ToArray()[1..])
                {
                    Assert.IsTrue(stat.ActivityState != lastState);
                    lastState = stat.ActivityState;
                }
            }
        }

        [TestMethod]
        [DataRow(12)]
        [DataRow(120)]
        [DataRow(120000)]
        public void TestStatsStartLessThanEnd(int statsDurationSec)
        {
            TimeSpan statsDuration = TimeSpan.FromSeconds(statsDurationSec);
            DateTime statsFrom = DateTime.Now - statsDuration;
            DateTime statsTo = DateTime.Now;

            var uam = UserActivityMonitor.GetInstance(TestAppName);
            var stats = uam.GetUserActivityStats(statsFrom, statsTo);

            foreach (var stat in stats.CompleteSessionList)
            {
                Assert.IsTrue(
                    stat.SessionStartTime < stat.SessionEndTime ||
                        stat.SessionEndTime == DateTime.MinValue);
            }
        }

        [TestMethod]
        [DataRow(12, 0)]
        [DataRow(120, 0)]
        [DataRow(120000, 0)]
        [DataRow(120000, 100)]
        public void TestStatsOnlyOneRowWithOpenEndTime(
                        int statsDurationSec, int monitoringCycles)
        {
            TimeSpan statsDuration = TimeSpan.FromSeconds(statsDurationSec);
            DateTime statsFrom = DateTime.Now - statsDuration;
            DateTime statsTo = DateTime.Now;

            var uam = UserActivityMonitor.GetInstance(TestAppName);

            if (monitoringCycles > 0)
            {
                uam.ClearAllUserActivityStats();
            }
            for (int i = 0; i < monitoringCycles; i++)
            {
                uam.StartMonitoring();
                uam.StopMonitoring();
                uam.GetUserActivityStats(
                    DateTime.Now.Subtract(TimeSpan.FromMinutes(30)), DateTime.Now);
            }

            var stats = uam.GetUserActivityStats(statsFrom, statsTo);
            bool openRowFound = false;
            foreach (var stat in stats.CompleteSessionList)
            {
                Assert.IsTrue(stat.SessionEndTime != DateTime.MinValue || !openRowFound);
                if (stat.SessionEndTime == DateTime.MinValue)
                {
                    openRowFound = true;
                }
            }
        }

        [TestMethod]
        public void TestInactivityCallback()
        {
            var uam = UserActivityMonitor.GetInstance(TestAppName);

            DateTime lastInputTime = DateTime.MinValue;
            DateTime begin = DateTime.Now;

            TimeSpan inactivityThreshold = TimeSpan.FromSeconds(10);

            UserInactiveCallbackType localCb = t => lastInputTime = t;

            uam.UserInactivityThreshold = inactivityThreshold;
            uam.UserInactiveCallback += localCb;

            Thread.Sleep(inactivityThreshold.Add(TimeSpan.FromSeconds(1)));
            Assert.IsTrue(lastInputTime == DateTime.MinValue, $"lastInputTime {lastInputTime}");

            uam.StartMonitoring();
            Thread.Sleep(inactivityThreshold.Add(TimeSpan.FromSeconds(1)));
            uam.StopMonitoring();

            Assert.IsTrue(lastInputTime != DateTime.MinValue, $"lastInputTime {lastInputTime}");

            lastInputTime = DateTime.MinValue;
            uam.UserInactiveCallback -= localCb;

            uam.StartMonitoring();
            Thread.Sleep(inactivityThreshold.Add(TimeSpan.FromSeconds(1)));
            uam.StopMonitoring();

            Assert.IsTrue(lastInputTime == DateTime.MinValue, $"lastInputTime {lastInputTime}");
        }

        private double GetStatsTotalDurationDelta(
            TimeSpan statsRequestedFor, UserActivityStats reportedStats)
        {
            TimeSpan totalReportedStatsDuration =
                reportedStats.TotalActiveTime + reportedStats.TotalInactiveTime + reportedStats.TotalUnmonitoredTime;
            return Math.Abs((statsRequestedFor - totalReportedStatsDuration).TotalSeconds);
        }

        private double GetStatsActiveInactiveDurationDelta(
            TimeSpan statsRequestedFor, UserActivityStats reportedStats)
        {
            TimeSpan activeInactiveReportedStatsDuration =
                reportedStats.TotalActiveTime + reportedStats.TotalInactiveTime;
            return Math.Abs((statsRequestedFor - activeInactiveReportedStatsDuration).TotalSeconds);
        }
    }
}
