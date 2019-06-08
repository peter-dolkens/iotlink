﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using IOTLink.Platform;
using IOTLink.Platform.Windows;

namespace IOTLink.Helpers
{
    public static class PlatformHelper
    {
        /// <summary>
        /// Return the machine name containing the domain is necessary
        /// </summary>
        /// <returns>String</returns>
        public static string GetFullMachineName()
        {
            string domainName = Environment.UserDomainName;
            string computerName = Environment.MachineName;
            if (domainName.Equals(computerName))
                return computerName;

            return string.Format("{0}\\{1}", domainName, computerName);
        }

        /// <summary>
        /// Return the username from the sessionId
        /// </summary>
        /// <param name="sessionId">Integer</param>
        /// <returns>String</returns>
        public static string GetUsername(int sessionId)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return WindowsAPI.GetUsername(sessionId);

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Execute a system shutdown
        /// </summary>
        /// <param name="force">Boolean indicating if the call should be flagged as forced</param>
        public static void Shutdown(bool force = false)
        {
            LoggerHelper.Debug("Executing {0} system shutdown.", force ? "forced" : "normal");
            string filename = "shutdown";
            string args = null;

            // Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                args = force ? "-s -f -t 0" : "-s -t 0";

            // Linux or OSX
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                args = "-h now";

            Process.Start(filename, args);
        }

        /// <summary>
        /// Execute a system reboot
        /// </summary>
        /// <param name="force">Boolean indicating if the call should be flagged as forced</param>
        public static void Reboot(bool force = false)
        {
            LoggerHelper.Debug("Executing {0} system shutdown.", force ? "forced" : "normal");
            string filename = "shutdown";
            string args = null;

            // Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                args = force ? "-r -f -t 0" : "-r -t 0";

            // Linux or OSX
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                args = "-r -h now";

            Process.Start(filename, args);
        }

        /// <summary>
        /// Puts the system into a hibernate state if possible
        /// </summary>
        public static void Hibernate()
        {
            LoggerHelper.Debug("Executing system hibernation.");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                WindowsAPI.Hibernate();

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Puts the system into a suspended state if possible
        /// </summary>
        public static void Suspend()
        {
            LoggerHelper.Debug("Executing system suspend.");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                WindowsAPI.Suspend();

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Logoff the user from the system
        /// </summary>
        /// <param name="username">User which needs be logged-off</param>
        public static void Logoff(string username)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException();

            if (string.IsNullOrWhiteSpace(username))
            {
                LoggerHelper.Debug("Executing Logoff on all users");
                WindowsAPI.LogoffAll();
            }
            else
            {
                LoggerHelper.Debug(string.Format("Executing Logoff on user {0}", username));
                WindowsAPI.LogOffUser(username);
            }
        }

        /// <summary>
        /// Lock the user session from the system
        /// </summary>
        /// <param name="username">User which needs be its session locked</param>
        public static void Lock(string username)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException();

            if (string.IsNullOrWhiteSpace(username))
            {
                LoggerHelper.Debug("Locking all users sessions");
                WindowsAPI.LockAll();
            }
            else
            {
                LoggerHelper.Debug(string.Format("Locking {0} user session", username));
                WindowsAPI.LockUser(username);
            }
        }

        /// <summary>
        /// Execute a system application
        /// </summary>
        /// <param name="command">Filename or command line</param>
        /// <param name="args">String containing all arguments</param>
        /// <param name="path">String containing the work path</param>
        /// <param name="username">String containing the user which the application will be executed</param>
        public static void Run(string command, string args, string path, string username)
        {
            if (!string.IsNullOrWhiteSpace(args))
                args = string.Format("{0} {1}", Path.GetFileName(command), args);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException();

            LoggerHelper.Debug("Run - Command: {0} Args: {1} Path: {2} User: {3}", command, args, path, username);
            WindowsAPI.Run(command, args, path, username);
        }

        /// <summary>
        /// Return a <see cref="MemoryInfo"/> object with all current memory information.
        /// </summary>
        /// <returns><see cref="MemoryInfo"/> object</returns>
        public static MemoryInfo GetMemoryInformation()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException();

            return WindowsAPI.GetMemoryInformation();
        }
    }
}