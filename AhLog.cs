using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace AngelHornetLibrary
{
    public class AhLog
    {
        // using static AngelHornetLibrary.AhLog;
        // Debug is the default log level
        // AhLog: Start, Stop, Log()*, ... LogLevels: LogTrace(), LogDebug()*, LogInformation(), LogWarning(), LogError(), LogCritical()
        // VRB, DBG, INF, WRN, ERR, FTL
        public static ILogger<AhLog> _ahLog { get; private set; } = null;
        public static LoggingLevelSwitch _LoggingLevel { get; set; } = new LoggingLevelSwitch();
        public static string _logFilePath { get; private set; } = string.Empty;



        // The first log message determines the log level.
        // AhLog: Start, Stop, Log, ... LogLevels: LogTrace, LogDebug*, LogInformation, LogWarning, LogError, LogCritical
        public static ILogger<AhLog> Start(LogEventLevel LogLevel)
        {
            if (_ahLog == null)
            {
                _LoggingLevel.MinimumLevel = LogLevel;
                var folder = Environment.SpecialFolder.ApplicationData;
                if (Debugger.IsAttached)
                    folder = Environment.SpecialFolder.Desktop;
                var path = Environment.GetFolderPath(folder);
                _logFilePath = Path.Join(path, "AhLogfile.log");
                Debug.WriteLine($"AhLog _logFilePath: {_logFilePath}");
                Debug.WriteLine($"AhLog LogLevel: {LogLevel}");
                if (File.Exists(_logFilePath))
                {
                    File.Delete(_logFilePath);
                }

                var services = new ServiceCollection()
                .AddLogging(builder =>
                {
                    var logger = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(_LoggingLevel)
                    .WriteTo.Debug()
                    .WriteTo.File(_logFilePath, shared: true, retainedFileCountLimit: 1, fileSizeLimitBytes: 1000000)
                    .CreateLogger();
                    builder.AddSerilog(logger);
                })
                .BuildServiceProvider();

                _ahLog = services.GetService<ILogger<AhLog>>();
                _ahLog.LogInformation($"AhLogInit: {LogLevel} {_logFilePath}");
                File.SetAttributes(_logFilePath, FileAttributes.Hidden);
            }

            return _ahLog;
        }
        




        public static void Log(string message) => LogDebug(message);
        public static void LogInfo(string message) => LogInformation(message);
        public static void LogMsg(string message) => LogInformation(message);
        public static void LogWarn(string message) => LogWarning(message);



        public static void LogTrace(string message)
        {
            Start(LogEventLevel.Verbose);
            if (_ahLog != null)
            {
                _ahLog.LogTrace(message);
            }
        }
        public static void LogDebug(string message)
        {
            Start(LogEventLevel.Debug);
            if (_ahLog != null)
            {
                _ahLog.LogDebug(message);
            }
        }
        public static void LogInformation(string message)
        {
            Start(LogEventLevel.Information);
            if (_ahLog != null)
            {
                _ahLog.LogInformation(message);
            }
        }
        public static void LogWarning(string message)
        {
            Start(LogEventLevel.Warning);
            if (_ahLog != null)
            {
                _ahLog.LogWarning(message);
            }
        }
        public static void LogError(string message)
        {
            Start(LogEventLevel.Error);
            if (_ahLog != null)
            {
                _ahLog.LogError(message);
            }
        }
        public static void LogCritical(string message)
        {
            Start(LogEventLevel.Fatal);
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

