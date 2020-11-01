using System;
using System.Threading;
using DotNetUtils.Win32.user32.dll;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetUtils.Win32.Test
{
    [TestClass]
    public class LastInputInfoTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            // last user input must be greater than 5 seconds since this test sleeps for 5 sec
            // Provided user doesn't interact with the system after launching this test
            Thread.Sleep(5000);
            uint userInput = LastInputInfo.GetMillisSinceLastUserInput();
            Console.WriteLine($"millis since last user input: {userInput}");
            Assert.IsTrue(userInput > 4000,
                $"Millis since last user input ({userInput}) is less than 4 seconds");
        }

        [TestMethod]
        public void TestMethod2()
        {
            uint millis = LastInputInfo.GetMillisSinceSystemBootup();
            int millisEnv = Environment.TickCount;
            int delta = Math.Abs(millisEnv - (int)millis);
            Console.WriteLine($"system bootup {millis} vs {millisEnv} delta {delta}");
            Assert.IsTrue(delta < 1000, $"millis since powerup delta from 2 sources ({delta}) more than 1 second.");
        }

        [TestMethod]
        [DataRow(60000)] // this should pass as long as the test executes 60 sec from last user input
        [DataRow(1000)] // this will pass if we keep moving the pointer during execution
        public void TestMethod3(int threshold)
        {
            DateTime lastUserInputTime = LastInputInfo.GetLastUserInputTime();
            int delta = (int)DateTime.Now.Subtract(lastUserInputTime).TotalMilliseconds;
            Assert.IsTrue(Math.Abs(delta) < threshold,
                $"now - last user input time = {delta} milliseconds, test threshold {threshold}");
        }
    }
}
