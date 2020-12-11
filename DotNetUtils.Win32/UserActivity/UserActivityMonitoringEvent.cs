using DotNetUtils.Win32.UserActivity.DB.Models;
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

            // Fetch the meta info row
            using var context = Factory.NewUserActivityContext();
            var metaInfo = context.UserActivityMetaInfoSet.Single();

            if (metaInfo == null)
            {
                Console.WriteLine("No row in UserActivity meta info");
            }
            else
            {
                Console.WriteLine("Found row in UserActivity meta info");
            }
        }
    }
}
