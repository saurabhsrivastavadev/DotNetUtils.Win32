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

            TimeSpan totalStatsDuration = stats.TotalActiveTime + stats.TotalInactiveTime +
                                            stats.TotalUnmonitoredTime;
            Assert.AreEqual(totalStatsDuration.TotalMilliseconds, 0);
        }

        [TestMethod]
        [DataRow(30)]
        public void TestClearAndGetStats(int monitorForSeconds)
        {
            var uam = new UserActivityMonitor(TestAppName);

            Factory.MonitoringInterval = TimeSpan.FromSeconds(5);
            Factory.UserConsideredInactiveAfter = TimeSpan.FromSeconds(10);

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
            var uam = new UserActivityMonitor(TestAppName);

            uam.StartMonitoring();

            Thread.Sleep(180000);

            uam.StopMonitoring();
        }

        [TestMethod]
        [DataRow(30, true)]
        [DataRow(30, false)]
        public void TestGetUserActivityStats(int statsDurationSec, bool startMonitoring)
        {
            Factory.MonitoringInterval = TimeSpan.FromSeconds(5);
            Factory.UserConsideredInactiveAfter = TimeSpan.FromSeconds(10);

            TimeSpan statsDuration = TimeSpan.FromSeconds(statsDurationSec);

            var uam = new UserActivityMonitor(TestAppName);

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
        [DataRow(12)]
        [DataRow(120)]
        [DataRow(120000)]
        public void TestNoConsecutiveRowsWithSameActivityState(int statsDurationSec)
        {
            TimeSpan statsDuration = TimeSpan.FromSeconds(statsDurationSec);
            DateTime statsFrom = DateTime.Now - statsDuration;
            DateTime statsTo = DateTime.Now;

            var uam = new UserActivityMonitor(TestAppName);
            var stats = uam.GetUserActivityStats(statsFrom, statsTo);

            DateTime lastEnd = DateTime.MinValue;
            foreach (var stat in stats.UserActiveSessionList)
            {
                Assert.IsTrue(stat.SessionStartTime != lastEnd);
                lastEnd = stat.SessionEndTime;
            }
            lastEnd = DateTime.MinValue;
            foreach (var stat in stats.UserInactiveSessionList)
            {
                Assert.IsTrue(stat.SessionStartTime != lastEnd);
                lastEnd = stat.SessionEndTime;
            }
            lastEnd = DateTime.MinValue;
            foreach (var stat in stats.UserUnmonitoredSessionList)
            {
                Console.WriteLine($"Assert {stat.SessionStartTime} != {lastEnd}");
                Assert.IsTrue(stat.SessionStartTime != lastEnd);
                lastEnd = stat.SessionEndTime;
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

            var uam = new UserActivityMonitor(TestAppName);
            var stats = uam.GetUserActivityStats(statsFrom, statsTo);

            foreach (var stat in stats.UserActiveSessionList)
            {
                Assert.IsTrue(
                    stat.SessionStartTime < stat.SessionEndTime ||
                        stat.SessionEndTime == DateTime.MinValue);
            }
            foreach (var stat in stats.UserInactiveSessionList)
            {
                Assert.IsTrue(
                    stat.SessionStartTime < stat.SessionEndTime ||
                        stat.SessionEndTime == DateTime.MinValue);
            }
            foreach (var stat in stats.UserUnmonitoredSessionList)
            {
                Assert.IsTrue(
                    stat.SessionStartTime < stat.SessionEndTime ||
                        stat.SessionEndTime == DateTime.MinValue);
            }
        }

        [TestMethod]
        [DataRow(12)]
        [DataRow(120)]
        [DataRow(120000)]
        public void TestStatsOnlyOneRowWithOpenEndTime(int statsDurationSec)
        {
            TimeSpan statsDuration = TimeSpan.FromSeconds(statsDurationSec);
            DateTime statsFrom = DateTime.Now - statsDuration;
            DateTime statsTo = DateTime.Now;

            var uam = new UserActivityMonitor(TestAppName);
            var stats = uam.GetUserActivityStats(statsFrom, statsTo);
            bool openRowFound = false;

            foreach (var stat in stats.UserActiveSessionList)
            {
                Assert.IsTrue(stat.SessionEndTime != DateTime.MinValue || !openRowFound);
                if (stat.SessionEndTime == DateTime.MinValue)
                {
                    openRowFound = true;
                }
            }
            foreach (var stat in stats.UserInactiveSessionList)
            {
                Assert.IsTrue(stat.SessionEndTime != DateTime.MinValue || !openRowFound);
                if (stat.SessionEndTime == DateTime.MinValue)
                {
                    openRowFound = true;
                }
            }
            foreach (var stat in stats.UserUnmonitoredSessionList)
            {
                Assert.IsTrue(stat.SessionEndTime != DateTime.MinValue || !openRowFound);
                if (stat.SessionEndTime == DateTime.MinValue)
                {
                    openRowFound = true;
                }
            }
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
