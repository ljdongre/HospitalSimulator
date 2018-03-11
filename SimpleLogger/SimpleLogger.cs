using SimpleLoggerContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLogger
{
    public class SimpleLogger : ISimpleLogger, IDisposable
    {
        public LogLevel CurrentLogLevel { get; private set; }
        public string   LogFile { get; private set; }
        StreamWriter _logWriter;
        SimpleLogger(LogLevel currentLevel)
        {
            CurrentLogLevel = currentLevel;
            LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "HSServiceLog.txt");
            _logWriter = new StreamWriter(LogFile);
        }

        public void Log(LogLevel level, string message,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (_logWriter != null && CurrentLogLevel >= level)
            {
                _logWriter.WriteLine(string.Format("{0} [{1}:{2}] | {3}", sourceFilePath, memberName, sourceLineNumber, message));
                _logWriter.Flush();
            }
        }

        public void Dispose()
        {
            if (_logWriter != null)
            {
                _logWriter.Flush();
                _logWriter.Close();
            }
        }

        private static readonly SimpleLogger _logInstance = new SimpleLogger(LogLevel.Info);

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static SimpleLogger()
        {
        }

        private SimpleLogger()
        {
        }

        public static SimpleLogger Instance
        {
            get
            {
                return _logInstance;
            }
        }
    }
}
