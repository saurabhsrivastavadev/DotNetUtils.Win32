using DotNetUtils.Win32.user32.dll;
using DotNetUtils.Win32.UserActivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace DotNetUtils.Win32.UserActivity
{
    public class UserActivityMonitor : IUserActivityMonitor
    {
        private Timer _monitoringTimer;

        public TimeSpan UserInactivityThreshold { get; set; }
        public UserInactiveCallbackType UserInactiveCallback { get; set; }

        public DateTime GetLastUserInputTime()
        {
            return LastInputInfo.GetLastUserInputTime();
        }

        public UserActivityStats GetUserActivityStats(DateTime StatsFrom, DateTime StatsTo)
        {
            throw new NotImplementedException();
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
        }

        public void StopMonitoring()

        {
            _monitoringTimer.Stop();
        }

        private void OnMonitoringTimerExpiry(object source, ElapsedEventArgs e)
        {
            UserActivityMonitoringEvent.ProcessUserActivity();
        }
    }
}
