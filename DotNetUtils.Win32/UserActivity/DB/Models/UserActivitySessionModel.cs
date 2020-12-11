using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Win32.UserActivity.DB.Models
{
    /// <summary>
    /// Model to represent a single user session.
    /// Can represent both user activity or inactivity sessions.
    /// </summary>
    class UserActivitySessionModel
    {
        public int Id { get; set; }

        public UserActivityState UserActivityState { get; set; }

        public DateTime SessionStartTime { get; set; }

        public DateTime SessionEndTime { get; set; }
    }

    enum UserActivityState
    {
        ACTIVE,
        INACTIVE,
        UNMONITORED
    }
}
