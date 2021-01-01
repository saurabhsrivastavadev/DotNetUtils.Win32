using DotNetUtils.Win32.user32.dll;
using DotNetUtils.Win32.UserActivity;
using DotNetUtils.Win32.UserActivity.DB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace DotNetUtils.Win32.UserActivity
{
    public class UserActivityMonitor : IUserActivityMonitor
    {
        private Timer _monitoringTimer;

        public TimeSpan UserInactivityThreshold { get; set; }
        public UserInactiveCallbackType UserInactiveCallback { get; set; }

        /// <summary>
        /// Create UserActivityMonitor instance by specifying the app name
        /// </summary>
        /// <param name="appName">
        /// Name of the application using DotNetUtils.Win32 library
        /// </param>
        public UserActivityMonitor(string appName)
        {
            Factory.AppName = appName;
        }

        public DateTime GetLastUserInputTime()
        {
            return LastInputInfo.GetLastUserInputTime();
        }

        public UserActivityStats GetUserActivityStats(DateTime statsFrom, DateTime statsTo)
        {
            if (statsFrom >= statsTo)
            {
                throw new ArgumentException("statsFrom must be < statsTo");
            }

            Console.WriteLine($"Getting stats from {statsFrom} to {statsTo}");

            // Fetch all rows for required range
            using var db = Factory.NewUserActivityContext();

            var statsDbRows = db.UserActivitySessionSet.AsNoTracking().Where(
                session => (statsFrom < session.SessionEndTime ||
                                session.SessionEndTime == DateTime.MinValue) &&
                            statsTo > session.SessionStartTime).
                            OrderBy(s => s.SessionStartTime).
                            ToList();

            if (statsDbRows == null || statsDbRows.Count == 0)
            {
                return new UserActivityStats(DateTime.MinValue, DateTime.MinValue,
                    TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, null, null, null);
            }

            DateTime dbStatsFrom = statsFrom > statsDbRows.First().SessionStartTime ?
                                        statsFrom : statsDbRows.First().SessionStartTime;
            DateTime dbStatsTo = statsTo < statsDbRows.Last().SessionEndTime ?
                                        statsTo : statsDbRows.Last().SessionEndTime;

            TimeSpan totalActiveTime = TimeSpan.Zero;
            TimeSpan totalInactiveTime = TimeSpan.Zero;
            TimeSpan totalUnmonitoredTime = TimeSpan.Zero;

            List<UserActivitySession> activeSessionList = new();
            List<UserActivitySession> inactiveSessionList = new();
            List<UserActivitySession> unmonitoredSessionList = new();

            foreach (var session in statsDbRows)
            {
                Console.WriteLine($"session row :: id {session.Id} " +
                    $"start {session.SessionStartTime} end {session.SessionEndTime}");

                TimeSpan sessionDuration = GetSessionDuration(
                    session,
                    object.ReferenceEquals(session, statsDbRows.First()),
                    object.ReferenceEquals(session, statsDbRows.Last()),
                    statsFrom, statsTo);

                Console.WriteLine($"session duration - {sessionDuration}");

                switch (session.UserActivityState)
                {
                    case UserActivityState.ACTIVE:
                        totalActiveTime += sessionDuration;
                        activeSessionList.Add(
                            new UserActivitySession(session.UserActivityState,
                                    session.SessionStartTime, session.SessionEndTime));
                        break;

                    case UserActivityState.INACTIVE:
                        totalInactiveTime += sessionDuration;
                        inactiveSessionList.Add(
                            new UserActivitySession(session.UserActivityState,
                                    session.SessionStartTime, session.SessionEndTime));
                        break;

                    case UserActivityState.UNMONITORED:
                        totalUnmonitoredTime += sessionDuration;
                        unmonitoredSessionList.Add(
                            new UserActivitySession(session.UserActivityState,
                                    session.SessionStartTime, session.SessionEndTime));
                        break;
                }
            }

            return new UserActivityStats(statsFrom, statsTo,
                totalActiveTime, totalInactiveTime, totalUnmonitoredTime,
                activeSessionList, inactiveSessionList, unmonitoredSessionList);
        }

        private TimeSpan GetSessionDuration(
            UserActivitySessionModel session, bool isFirstSession, bool isLastSession,
            DateTime statsFrom, DateTime statsTo)
        {
            DateTime sessionEnd = session.SessionEndTime == DateTime.MinValue ?
                                            DateTime.Now : session.SessionEndTime;
            DateTime sessionStart = session.SessionStartTime;

            TimeSpan sessionDuration = sessionEnd - sessionStart;

            if (isFirstSession)
            {
                sessionDuration = sessionEnd - statsFrom;
            }
            else if (isLastSession)
            {
                sessionDuration = statsTo - sessionStart;
            }

            if (sessionDuration > sessionEnd - sessionStart)
            {
                sessionDuration = sessionEnd - sessionStart;
            }

            return sessionDuration;
        }

        public void StartMonitoring()
        {
            // start a timer and perform monitoring processing on each timer expiry
            if (_monitoringTimer == null)
            {
                _monitoringTimer = new Timer(5000);
                _monitoringTimer.Elapsed += OnMonitoringTimerExpiry;
                _monitoringTimer.AutoReset = true;
            }

            _monitoringTimer.Start();

            // explicit call to avoid first timer expiry delay
            UserActivityMonitoringEvent.ProcessUserActivity();
        }

        public void StopMonitoring()
        {
            _monitoringTimer.Stop();
            UserActivityMonitoringEvent.ProcessExplicitStopMonitoringCall();
        }

        private void OnMonitoringTimerExpiry(object source, ElapsedEventArgs e)
        {
            UserActivityMonitoringEvent.ProcessUserActivity();
        }

        public async void ClearAllUserActivityStats()
        {
            using var db = Factory.NewUserActivityContext();

            var metaInfoSet = await db.UserActivityMetaInfoSet.AsQueryable().ToListAsync();
            Console.WriteLine($"Deleting all {metaInfoSet.Count} elements in UserActivityMetaInfo Table");
            db.UserActivityMetaInfoSet.RemoveRange(metaInfoSet);

            var sessionSet = await db.UserActivitySessionSet.AsQueryable().ToListAsync();
            Console.WriteLine($"Found {sessionSet.Count} elements in UserActivitySession Table");
            db.UserActivitySessionSet.RemoveRange(sessionSet);

            await db.SaveChangesAsync();
        }
    }
}
