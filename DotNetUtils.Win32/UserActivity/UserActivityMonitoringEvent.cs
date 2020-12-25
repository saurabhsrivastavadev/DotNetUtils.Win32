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
        public static void ProcessUserActivity()
        {
            Console.WriteLine("ProcessUserActivity()");

            DateTime lastMonitoringEvent = MetaInfoUpdate();
            SessionUpdate(lastMonitoringEvent);
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

            metaInfo.LatestMonitoringEventTime = DateTime.Now;
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
                if (IsUserUnmonitored(lastMonitoringEvent))
                {
                    latestSession.SessionEndTime = lastMonitoringEvent;
                    db.UserActivitySessionSet.Add(NewUnmonitoredSessionRow(lastMonitoringEvent));
                }
                else if (latestSession.UserActivityState != GetCurrentUserActivityState())
                {
                    latestSession.SessionEndTime = DateTime.Now;
                    db.UserActivitySessionSet.Add(NewActiveInactiveSessionRow());
                }
            }

            db.SaveChanges();
        }

        private static UserActivitySessionModel NewUnmonitoredSessionRow(DateTime lastMonitoringEvent)
        {
            var session = Factory.NewUserActivitySessionModel();
            session.UserActivityState = UserActivityState.UNMONITORED;
            session.SessionStartTime = lastMonitoringEvent;
            session.SessionEndTime = DateTime.Now;
            return session;
        }

        private static UserActivitySessionModel NewActiveInactiveSessionRow()
        {
            var session = Factory.NewUserActivitySessionModel();
            session.UserActivityState = GetCurrentUserActivityState();
            session.SessionStartTime = DateTime.Now;
            return session;
        }

        private static bool IsUserUnmonitored(DateTime lastMonitoringEvent) =>
            lastMonitoringEvent != DateTime.MinValue &&
            DateTime.Now.Subtract(lastMonitoringEvent) > (2 * Factory.MonitoringInterval);

        private static bool IsUserActive() =>
            (LastInputInfo.GetMillisSinceLastUserInput() <
                    Factory.UserConsideredInactiveAfter.TotalMilliseconds);

        private static UserActivityState GetCurrentUserActivityState() =>
            IsUserActive() ? UserActivityState.ACTIVE : UserActivityState.INACTIVE;
    }
}
