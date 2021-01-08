using DotNetUtils.Win32.user32.dll;
using DotNetUtils.Win32.UserActivity.DB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Win32.UserActivity
{
    /// <summary>
    /// This class handles the processing of a single monitoring event.
    /// </summary>
    static class UserActivityMonitoringEvent
    {
        private static DateTime CurrentEventTimestamp { get; set; }

        private static readonly object _lock = DateTime.MinValue;

        public static void ProcessUserActivity()
        {
            lock(_lock)
            {
                Console.WriteLine("ProcessUserActivity()");

                CurrentEventTimestamp = DateTime.Now;
                DateTime lastMonitoringEvent = MetaInfoUpdate();
                SessionUpdate(lastMonitoringEvent);
                DbSanityCheck();
            }
        }

        public static void ProcessExplicitStopMonitoringCall()
        {
            lock(_lock)
            {
                Console.WriteLine("ProcessExplicitStopMonitoringCall()");

                using var db = Factory.NewUserActivityContext();

                if (db.UserActivitySessionSet.Any())
                {
                    int maxId = db.UserActivitySessionSet.Max(session => session.Id);
                    var latestSession = db.UserActivitySessionSet.Where(s => (s.Id == maxId)).First();

                    if (latestSession.UserActivityState == UserActivityState.ACTIVE ||
                            latestSession.UserActivityState == UserActivityState.INACTIVE)
                    {
                        latestSession.SessionEndTime = DateTime.Now;
                        var newSession = NewUnmonitoredSessionRow(DateTime.Now);
                        newSession.SessionEndTime = DateTime.MinValue;
                        db.UserActivitySessionSet.Add(newSession);
                    }
                }

                db.SaveChanges();
            }
        }

        private static DateTime MetaInfoUpdate()
        {
            using var db = Factory.NewUserActivityContext();

            DateTime lastMonitoringEvent = DateTime.MinValue;
            UserActivityMetaInfoModel metaInfo;
            if (!db.UserActivityMetaInfoSet.Any())
            {
                metaInfo = Factory.NewUserActivityMetaInfoModel();
                db.UserActivityMetaInfoSet.Add(metaInfo);
            }
            else
            {
                metaInfo = db.UserActivityMetaInfoSet.Single();
                lastMonitoringEvent = metaInfo.LatestMonitoringEventTime;
            }

            metaInfo.LatestMonitoringEventTime = CurrentEventTimestamp;
            db.SaveChanges();

            return lastMonitoringEvent;
        }

        private static void SessionUpdate(DateTime lastMonitoringEvent)
        {
            using var db = Factory.NewUserActivityContext();

            // Is user active or inactive
            bool isUserActive = IsUserActive();
            Console.WriteLine($"Is user active = {isUserActive}");

            // If no session row, add a new session row for isUserActive
            if (!db.UserActivitySessionSet.Any())
            {
                db.UserActivitySessionSet.Add(NewActiveInactiveSessionRow());
            }
            else
            {
                int maxId = db.UserActivitySessionSet.Max(session => session.Id);
                var latestSession = db.UserActivitySessionSet.Where(s => (s.Id == maxId)).First();
                if (IsUserUnmonitored(lastMonitoringEvent) &&
                        latestSession.UserActivityState != UserActivityState.UNMONITORED)
                {
                    latestSession.SessionEndTime = lastMonitoringEvent;
                    db.UserActivitySessionSet.Add(NewUnmonitoredSessionRow(lastMonitoringEvent));
                }
                else if (latestSession.UserActivityState != GetCurrentUserActivityState())
                {
                    latestSession.SessionEndTime = CurrentEventTimestamp;
                    db.UserActivitySessionSet.Add(NewActiveInactiveSessionRow());
                }
            }

            db.SaveChanges();
        }

        private static void DbSanityCheck()
        {
            // look for rows with same start/end time
            using var db = Factory.NewUserActivityContext();

            var sameStartEndRows = db.UserActivitySessionSet.Where(
                session => (session.SessionStartTime == session.SessionEndTime)).
                OrderBy(s => s.SessionStartTime).ToList();
            db.RemoveRange(sameStartEndRows);

            // look for more than one open rows
            var openRows = db.UserActivitySessionSet.Where(
                session => (session.SessionEndTime == DateTime.MinValue)).
                OrderByDescending(s => s.Id).ToList();
            if (openRows.Count > 1)
            {
                // Remove all except latest open row
                db.RemoveRange(openRows.Skip(1));
            }

            db.SaveChanges();
        }

        private static UserActivitySessionModel NewUnmonitoredSessionRow(DateTime lastMonitoringEvent)
        {
            var session = Factory.NewUserActivitySessionModel();
            session.UserActivityState = UserActivityState.UNMONITORED;
            session.SessionStartTime = lastMonitoringEvent;
            session.SessionEndTime = CurrentEventTimestamp;
            return session;
        }

        private static UserActivitySessionModel NewActiveInactiveSessionRow()
        {
            var session = Factory.NewUserActivitySessionModel();
            session.UserActivityState = GetCurrentUserActivityState();
            session.SessionStartTime = CurrentEventTimestamp;
            return session;
        }

        private static bool IsUserUnmonitored(DateTime lastMonitoringEvent) =>
            lastMonitoringEvent != DateTime.MinValue &&
            CurrentEventTimestamp.Subtract(lastMonitoringEvent) > (2 * Factory.MonitoringInterval);

        private static bool IsUserActive() =>
            (LastInputInfo.GetMillisSinceLastUserInput() <
                    Factory.UserConsideredInactiveAfter.TotalMilliseconds);

        private static UserActivityState GetCurrentUserActivityState() =>
            IsUserActive() ? UserActivityState.ACTIVE : UserActivityState.INACTIVE;
    }
}
