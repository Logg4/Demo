using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace LogWriterLib
{
    public static class Log
    {
        private static Logger _logger = null;
        private static string _logFile = "";
        private static string _assemblyAndVersionStr = "LogWriterLib";
        private static readonly object _lock = new object();

        static Log()
        {
            try
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fvi.FileVersion;
                _assemblyAndVersionStr = assembly.GetName().Name +
                        " " + version;
            }
            catch { }
        }


        public static string LogFile
        {
            get
            {
                return _logFile;
            }
            set
            {
                if (value != null)
                {
                    _logFile = value;
                    lock (_lock)
                    {
                        Logger = new Logger(_logFile);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static Logger Logger
        {
            get
            {
                lock (_lock)
                {
                    SetCaller();
                    return _logger;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (value != null)
                    {
                        _logger = value;
                    }
                }
            }
        }

        public static bool IsDebug
        {
            get; set;
        }

        /// <summary>
        /// wrap
        /// </summary>
        /// <param name="msg"></param>
        public static void Debug(object msg)
        {
            if (Logger != null && IsDebug)
            {
                lock (_lock)
                {
                    Logger.Debug(msg);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        public static void Debug(object msg, Exception e)
        {
            if (Logger != null && IsDebug)
            {
                lock (_lock)
                {
                    Logger.Debug(msg, e);
                }
            }
        }

        /// <summary>
        /// wrap
        /// </summary>
        /// <param name="msg"></param>
        public static void Info(object msg)
        {
            if (Logger != null)
            {
                lock (_lock)
                {
                    Logger.Info(msg);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        public static void Info(object msg, Exception e)
        {
            if (Logger != null)
            {
                lock (_lock)
                {
                    Logger.Info(msg, e);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void Warn(object msg)
        {
            if (Logger != null)
            {
                lock (_lock)
                {
                    Logger.Warn(msg);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        public static void Warn(object msg, Exception e)
        {
            if (Logger != null)
            {
                lock (_lock)
                {
                    Logger.Warn(msg, e);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void Error(object msg)
        {
            if (Logger != null)
            {
                lock (_lock)
                {
                    Logger.Error(msg);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        public static void Error(object msg, Exception e)
        {
            if (Logger != null)
            {
                lock (_lock)
                {
                    Logger.Error(msg, e);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void SetCaller()
        {
            try
            {
                if (_logger == null)
                {
                    return;
                }
                // store caller info
                StackTrace stack = new StackTrace();
                if (stack != null)
                {
                    System.Reflection.MethodBase methodInfo = null;

                    if (stack.FrameCount > 3)
                    {
                        // third in the stack should be the desired method
                        methodInfo = stack.GetFrame(3).GetMethod();
                        _logger.SetCaller(_assemblyAndVersionStr, methodInfo.ReflectedType.FullName + "." + methodInfo.Name);
                    }
                    else
                    {
                        _logger.SetCaller(_assemblyAndVersionStr, "Unknown");
                    }
                }
            }
            catch (Exception)
            {
                _logger.SetCaller(_assemblyAndVersionStr, "Unknown");
            }
        }
    }
}
