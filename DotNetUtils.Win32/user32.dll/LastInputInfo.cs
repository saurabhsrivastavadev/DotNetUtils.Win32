using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DotNetUtils.Win32.user32.dll
{
    public class LastInputInfo
    {
        private static LASTINPUTINFO _lastInputInfo = new LASTINPUTINFO();

        /// <summary>
        /// Milliseconds since last user interaction with the system
        /// </summary>
        public static uint GetMillisSinceLastUserInput()
        {
            int systemUptime = Environment.TickCount;
            int idleTicks = 0;
            _lastInputInfo.cbSize = (uint)Marshal.SizeOf(_lastInputInfo);
            _lastInputInfo.dwTime = 0;
            if (GetLastInputInfo(ref _lastInputInfo))
            {
                int lastInputTicks = (int)_lastInputInfo.dwTime;
                idleTicks = systemUptime - lastInputTicks;
            }
            return (idleTicks > 0) ? (uint)idleTicks : 0;
        }

        public static DateTime GetLastUserInputTime()
        {
            uint millisSinceUserInput = GetMillisSinceLastUserInput();
            return DateTime.Now.Subtract(TimeSpan.FromMilliseconds(millisSinceUserInput));
        }

        /// <summary>
        /// Wrapper over native method to fetch milliseconds elapsed since
        /// system bootup.
        /// </summary>
        public static uint GetMillisSinceSystemBootup()
        {
            // equivalent to Environment.TickCount
            return GetTickCount();
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dwTime;
        }

        /// <summary>
        /// Native call to get last user interaction with system
        /// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getlastinputinfo
        /// https://www.pinvoke.net/default.aspx/user32/GetLastInputInfo.html
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        /// <summary>
        /// Native call to get system bootup time.
        /// https://docs.microsoft.com/en-us/windows/win32/api/sysinfoapi/nf-sysinfoapi-gettickcount
        /// </summary>
        [DllImport("kernel32.dll")]
        private static extern UInt32 GetTickCount();
    }
}
