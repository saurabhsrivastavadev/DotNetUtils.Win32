using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Win32.UserActivity.DB.Models
{
    /// <summary>
    /// Model for saving user activity monitoring meta info.
    /// This Model will have only one row in the table.
    /// </summary>
    class UserActivityMetaInfoModel
    {
        public int Id { get; set; }

        /// <summary>
        /// When did the app last monitor user activity.
        /// This field is to track when app stopped monitoring.
        /// Might be user requested or app was stopped.
        /// </summary>
        public DateTime LatestMonitoringEventTime { get; set; }
    }
}
