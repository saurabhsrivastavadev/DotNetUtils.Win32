using DotNetUtils.Win32.UserActivity.DB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Win32.UserActivity.DB
{
    class UserActivityContext : DbContext
    {
        public DbSet<UserActivityMetaInfoModel> UserActivityMetaInfoSet { get; set; }
        public DbSet<UserActivitySessionModel> UserActivitySessionSet { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=useractivity.db");
        }
    }
}
