using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace LogWriterLib
{
    public class Logger
    {
        private string _mashineName = "";
        private string _assemblyInfo = "";
        private string _methodInfo = "";
        private string _logFile = "";
        private string _level = "";

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="loggerName"></param>
        public Logger(string fileName)
        {
            _mashineName = Environment.MachineName;
            _logFile = fileName;
        }

        /// <summary>
        /// Header info for each output line
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="methodInfo"></param>
        public void SetCaller(string assemblyName, string methodInfo)
        {
            try
            {
                _assemblyInfo = assemblyName;
                _methodInfo = methodInfo;
            }
            catch { }
        }

        public bool WithMilliseconds
        {
            get; set;
        }

        private void Write(string msg)
        {
            try
            {
                if (!String.IsNullOrEmpty(_logFile))
                {
                    using (var fs = new FileStream(_logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (StreamWriter writer = new StreamWriter(fs))
                        {
                            string logMsg = "";
                            if (WithMilliseconds)
                            {
                                logMsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + " " + _mashineName;
                                logMsg += " [" + _assemblyInfo + "] " + "[" + _level + "] " + "<" + _methodInfo + "> - " + msg;
                            }
                            else
                            {
                                logMsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " " + _mashineName;
                                logMsg += " [" + _assemblyInfo + "] " + "[" + _level + "] " + "<" + _methodInfo + "> - " + msg;
                            }
                            writer.WriteLine(logMsg);
                            writer.Flush();
                        }
                    }
                }
                else
                {
                    EventLog appLog = new EventLog();
                    appLog.Source = _assemblyInfo;
                    appLog.WriteEntry(msg);
                }
            }
            catch (Exception x)
            {
                try
                {
                    EventLog appLog = new EventLog();
                    appLog.Source = _assemblyInfo;
                    string errMsg = "";
                    while (x != null)
                    {
                        errMsg += Environment.NewLine + x.Source + ":" + Environment.NewLine + x.Message +
                                Environment.NewLine + x.StackTrace + Environment.NewLine;
                        x = x.InnerException;
                    }
                    appLog.WriteEntry(msg + "\nNot written to logfile, because: [" + errMsg + "]");
                }
                catch { }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        private void Write(string msg, Exception x = null)
        {
            try
            {
                while (x != null)
                {
                    msg += Environment.NewLine + (x?.Source ?? "") + ":" + Environment.NewLine + (x?.Message ?? "") +
                            Environment.NewLine + (x?.StackTrace ?? "") + Environment.NewLine;
                    x = x.InnerException;
                }
                Write(msg);
            }
            catch
            {
                Write(msg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetLogLocation()
        {
            return _logFile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void Debug(object msg, Exception x = null)
        {
            _level = "DEBUG";
            Write(msg.ToString(), x);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void Info(object msg, Exception x = null)
        {
            _level = "INFO";
            Write(msg.ToString(), x);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void Warn(object msg, Exception x = null)
        {
            _level = "WARNING";
            Write(msg.ToString(), x);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void Error(object msg, Exception x = null)
        {
            _level = "ERROR";
            Write(msg.ToString(), x);
        }
    }
}
