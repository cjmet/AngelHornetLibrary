using Microsoft.Extensions.Logging;
using Serilog;
using System.Diagnostics;

namespace AngelHornetLibrary
{
    public class AhLog
    {
        // using static AngelHornetLibrary.AhLog;
        // AhLog: Start, Stop, Log(), ... LogLevels: LogTrace(), LogDebug(), LogInformation()*, LogWarning(), LogError(), LogCritical()
        // Information is the default log level
        public static ILogger<AhLog> _ahLog { get; private set; } = null;
        private static string _logFilePath { get; set; } = string.Empty;
        public static ILogger<AhLog> Start()
        {
            if (_ahLog == null)
            {
                var folder = Environment.SpecialFolder.ApplicationData;
#if DEBUG
                folder = Environment.SpecialFolder.Desktop;
#endif
                var path = Environment.GetFolderPath(folder);
                _logFilePath = Path.Join(path, "AhLogfile.log");
                Debug.WriteLine($"AhLog _logFilePath: {_logFilePath}");
                if (File.Exists(_logFilePath))
                {
                    File.Delete(_logFilePath);
                }

                var services = new ServiceCollection()
                .AddLogging(builder =>
                {
                    var logger = new LoggerConfiguration()
                    .WriteTo.Debug()
                    .WriteTo.File(_logFilePath, shared: true, retainedFileCountLimit: 1, fileSizeLimitBytes: 1000000)
                    .CreateLogger();
                    builder.AddSerilog(logger);
                })
                .BuildServiceProvider();
                _ahLog = services.GetService<ILogger<AhLog>>();
                _ahLog.LogInformation("AhLog Default Logger Started!");


            }

            return _ahLog;
        }
        // AhLog: Start, Stop, Log, ... LogLevels: LogTrace, LogDebug, LogInformation*, LogWarning, LogError, LogCritical

        public static void Log(string message) => LogInformation(message);
        public static void LogInfo(string message) => LogInformation(message);
        public static void LogInformation(string message)
        {
            Start();
            if (_ahLog != null)
            {
                _ahLog.LogInformation(message);
            }
        }
        public static void LogDebug(string message)
        {
            Start();
            if (_ahLog != null)
            {
                _ahLog.LogDebug(message);
            }
        }
        public static void LogError(string message)
        {
            Start();
            if (_ahLog != null)
            {
                _ahLog.LogError(message);
            }
        }
        public static void LogTrace(string message)
        {
            Start();
            if (_ahLog != null)
            {
                _ahLog.LogTrace(message);
            }
        }
        public static void LogWarning(string message)
        {
            Start();
            if (_ahLog != null)
            {
                _ahLog.LogWarning(message);
            }
        }
        public static void LogCritical(string message)
        {
            Start();
            if (_ahLog != null)
            {
                _ahLog.LogCritical(message);
            }
        }
        public static void Stop() => CloseAndDelete();
        public static void CloseAndDelete()
        {
            if (_ahLog != null)
            {
                _ahLog.LogInformation("[43] Goodbye, Dependency Injected AhLog Serilog!");
                Serilog.Log.CloseAndFlush();
                if (File.Exists(_logFilePath))
                {
                    File.Delete(_logFilePath);
                }
                _ahLog = null;
                _logFilePath = string.Empty;
            }
        }
    }
}

