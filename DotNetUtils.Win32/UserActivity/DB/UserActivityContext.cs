using DotNetUtils.Win32.UserActivity.DB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DotNetUtils.Win32.Test")]
namespace DotNetUtils.Win32.UserActivity.DB
{
    class UserActivityContext : DbContext
    {
        public DbSet<UserActivityMetaInfoModel> UserActivityMetaInfoSet { get; set; }
        public DbSet<UserActivitySessionModel> UserActivitySessionSet { get; set; }

        private readonly string _userActivityDir;
        private readonly string _dbFilePath;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbFilePath}");
        }

        public UserActivityContext()
        {
            // DB file path
            string rootDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string parentDir = Path.Combine(rootDir, @"sauviapps");
            string appDir = Path.Combine(parentDir, @"dotnetutils.win32");
            _userActivityDir = Path.Combine(appDir, @"useractivity");
            _dbFilePath = Path.Combine(_userActivityDir, @"useractivity.db");

            Console.WriteLine($"DB File Path: {_dbFilePath}");

            Init();
        }

        public void Init()
        {
            // Create directory to contain the DB file
            if (!Directory.Exists(_userActivityDir))
            {
                Directory.CreateDirectory(_userActivityDir);
            }

            // Apply DB Migration
            if (!File.Exists(_dbFilePath))
            {
                Console.WriteLine("useractivity.db file not present, create by applying migration.");
                Database.Migrate();
            }
            else
            {
                Console.WriteLine("useractivity.db file present.");
            }
        }
    }
}
