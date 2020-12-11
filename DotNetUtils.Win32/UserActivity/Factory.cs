using DotNetUtils.Win32.UserActivity.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Win32.UserActivity
{
    class Factory
    {
        public static UserActivityContext NewUserActivityContext()
        {
            return new UserActivityContext();
        }
    }
}
