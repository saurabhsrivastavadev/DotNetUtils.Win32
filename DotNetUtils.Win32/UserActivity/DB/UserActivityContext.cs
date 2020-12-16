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
        private readonly string _userActivityDbFilePath;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_userActivityDbFilePath}");
        }

        public UserActivityContext()
        {
            // DB file path
            string rootDirName = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);
            string appDirName = Factory.AppName;
            string libDirName = "dotnetutils.win32";
            string userActivityDirName = "useractivity";
            string userActivityDbFileName = "useractivity.db";
            _userActivityDir = Path.Combine(
                rootDirName, appDirName, libDirName, userActivityDirName);
            _userActivityDbFilePath = Path.Combine(_userActivityDir, userActivityDbFileName);

            Console.WriteLine($"DB File Path: {_userActivityDbFilePath}");

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
            if (!File.Exists(_userActivityDbFilePath))
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
