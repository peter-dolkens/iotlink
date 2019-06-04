﻿using System;
using System.IO;
using System.Timers;

namespace WinIOTLink.Helpers
{
    public class LoggerHelper
    {
        private static LoggerHelper _instance;
        private StreamWriter _logWriter;
        private Timer _flushTimer;

        public enum LogLevel
        {
            DISABLED,
            CRITICAL,
            ERROR,
            WARNING,
            INFO,
            DEBUG,
            TRACE,
            HELP_ME
        }

        public static LoggerHelper GetInstance()
        {
            if (_instance == null)
                _instance = new LoggerHelper();

            return _instance;
        }

        private LoggerHelper()
        {
            try
            {
                string logsPath = PathHelper.LogsPath();
                if (!Directory.Exists(logsPath))
                    Directory.CreateDirectory(logsPath);

                string path = logsPath + "\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".log";

                if (!File.Exists(path))
                    _logWriter = File.CreateText(path);
                else
                    _logWriter = File.AppendText(path);

                _flushTimer = new Timer();
                _flushTimer.Interval = 1500;
                _flushTimer.Elapsed += OnFlushInterval;
                _flushTimer.Enabled = true;
            }
            catch (Exception)
            {
                //TODO: Cry
            }
        }

        ~LoggerHelper()
        {
            try
            {
                if (_logWriter != null)
                {
                    _logWriter.Flush();
                    _logWriter.Close();
                }
            }
            catch (Exception)
            {
                //TODO: Cry
            }
        }

        public void Flush()
        {
            try
            {
                if (_logWriter != null)
                    _logWriter.Flush();
            }
            catch (Exception)
            {
                //TODO: Cry again
            }
        }

        private void WriteFile(string message)
        {
            try
            {
                if (_logWriter != null)
                    _logWriter.WriteLine(message);
            }
            catch (Exception)
            {
                //TODO: Cry again
            }
        }

        private void OnFlushInterval(object sender, ElapsedEventArgs e)
        {
            Flush();
        }

        private void WriteLog(LogLevel logLevel, Type origin, string message, params object[] args)
        {
            if (origin == null || string.IsNullOrWhiteSpace(message))
                return;

            string messageTag = origin.Name;
            string formatedMessage;
            if (args == null || args.Length == 0)
                formatedMessage = message;
            else
                formatedMessage = string.Format(message, args);

            string finalMessage = string.Format("[{0}][{1}][{2}][{3}]: {4}", WindowsHelper.GetFullMachineName(), DateTime.Now, logLevel.ToString(), messageTag, formatedMessage);
            WriteFile(finalMessage);
        }

        public static void Critical(Type origin, string message, params object[] args)
        {
            GetInstance().WriteLog(LogLevel.CRITICAL, origin, message, args);
        }

        public static void Error(Type origin, string message, params object[] args)
        {
            GetInstance().WriteLog(LogLevel.CRITICAL, origin, message, args);
        }

        public static void Warn(Type origin, string message, params object[] args)
        {
            GetInstance().WriteLog(LogLevel.CRITICAL, origin, message, args);
        }

        public static void Info(Type origin, string message, params object[] args)
        {
            GetInstance().WriteLog(LogLevel.CRITICAL, origin, message, args);
        }

        public static void Debug(Type origin, string message, params object[] args)
        {
            GetInstance().WriteLog(LogLevel.CRITICAL, origin, message, args);
        }

        public static void Trace(Type origin, string message, params object[] args)
        {
            GetInstance().WriteLog(LogLevel.CRITICAL, origin, message, args);
        }
    }
}
