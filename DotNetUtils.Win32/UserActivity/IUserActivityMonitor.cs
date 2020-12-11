using System;
using System.Collections.Generic;

namespace DotNetUtils.Win32.UserActivity
{
    public interface IUserActivityMonitor
    {
        /// <summary>
        /// This method fetches the time of last user interaction with the
        /// system.
        /// </summary>
        /// <returns>
        /// The DateTime instance when the user last interacted with the system.
        /// </returns>
        DateTime GetLastUserInputTime();

        /// <summary>
        /// Threshold after which user in considered inactive.
        /// </summary>
        TimeSpan UserInactivityThreshold { get; set; }

        /// <summary>
        /// The Callback delegate invoked when user has been inactive for
        /// the specified threshold <see cref="UserInactivityThreshold"/>.
        /// </summary>
        UserInactiveCallbackType UserInactiveCallback { get; set; }

        /// <summary>
        /// This method starts monitoring the user activity.
        /// </summary>
        void StartMonitoring();

        /// <summary>
        /// This method stops the user activity monitoring.
        /// </summary>
        void StopMonitoring();

        UserActivityStats GetUserActivityStats(DateTime StatsFrom, DateTime StatsTo);
    }

    public delegate void UserInactiveCallbackType(DateTime lastUserInputTime);

    public record UserActivitySession(DateTime SessionStartTime, DateTime SessionEndTime);

    public record UserActivityStats(
        DateTime StatsFrom, DateTime StatsTo,
        TimeSpan TotalActiveTime, TimeSpan TotalInactiveTime, TimeSpan TotalUnmonitoredTime,
        List<UserActivitySession> UserActiveSessionList, List<UserActivitySession> UserInactiveSessionList,
        List<UserActivitySession> UserUnmonitoredSessionList);
}
