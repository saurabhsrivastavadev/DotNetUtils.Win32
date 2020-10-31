using System;

namespace DotNetUtils.Win32
{
    public interface IUserActivity
    {
        /// <summary>
        /// This method fetches the time of last user interaction with the
        /// system.
        /// </summary>
        /// <returns>
        /// The time returned is in milliseconds since the system bootup.
        /// </returns>
        uint GetLastUserInputTime();

        /// <summary>
        /// Threshold for user inactivity in seconds when to invoke the
        /// UserInactiveCallback
        /// </summary>
        uint UserInactivityThresholdSec { get; set; }

        delegate void UserInactiveCallbackType();

        /// <summary>
        /// The Callback delegate invoked when user has been inactive for
        /// the specified threshold.
        /// </summary>
        UserInactiveCallbackType UserInactiveCallback { get; set; }
    }
}
