using DotNetUtils.Win32.UserActivity.DB;
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

        internal static UserActivityContext NewUserActivityContext()
        {
            return new UserActivityContext();
        }
    }
}
