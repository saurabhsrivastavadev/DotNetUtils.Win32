using DotNetUtils.Win32.UserActivity.DB;
using DotNetUtils.Win32.UserActivity.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Win32.UserActivity
{
    public static class Factory
    {
        public static string AppName
        {
            get => (!string.IsNullOrWhiteSpace(_appName)) ? _appName :
                throw new Exception(
                    "Factory.AppName must be set before using DotNetUtils.Win32 library APIs");
            set => _appName = (!string.IsNullOrWhiteSpace(value)) ? value.Trim() :
                throw new ArgumentException("Factory.AppName cannot be null or whitespace");
        }
        private static string _appName;

        public static TimeSpan MonitoringInterval { get; set; } = TimeSpan.FromMinutes(1);

        public static TimeSpan UserConsideredInactiveAfter { get; set; } = TimeSpan.FromMinutes(2);

        internal static UserActivityContext NewUserActivityContext() => new UserActivityContext();

        internal static UserActivityMetaInfoModel NewUserActivityMetaInfoModel() => new UserActivityMetaInfoModel();

        internal static UserActivitySessionModel NewUserActivitySessionModel() => new UserActivitySessionModel();
    }
}
