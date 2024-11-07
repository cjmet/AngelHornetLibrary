using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using System.Diagnostics;

namespace AngelHornetLibrary
{
    public class AhLog
    {
        // using static AngelHornetLibrary.AhLog;
        // AhLog: Start, Stop, Log(), ... LogLevels: LogTrace(), LogDebug()*, LogInformation(), LogWarning(), LogError(), LogCritical()
        // Synonym:  Log(), LogInfo(), LogMsg(), LogInformation()
        // Synonym:  LogWarn(), LogWarning()
        // Start(), or The first log message determines the log level.
        // Log*(True) will temporarily set the log level to that level.
        // Log*(False) will reset the log level to the default level.
        // Log Codes: VRB, DBG, INF, WRN, ERR, FTL

        public static ILogger<AhLog> _ahLog { get; private set; } = null;
        public static LoggingLevelSwitch _LoggingLevel { get; set; } = new LoggingLevelSwitch();
        public static string _logFilePath { get; private set; } = string.Empty;
        public static string _logFileName { get; private set; } = "AhLogfile.log";
        public static LogEventLevel _defaultLogLevel { get; private set; } = LogEventLevel.Information;


        public static ILogger<AhLog> Start(LogEventLevel LogLevel, string? FileName = null)
        {
            if (_ahLog == null)
            {
                _LoggingLevel.MinimumLevel = LogLevel;
                //_LoggingLevel.MinimumLevelChanged += (s, e) =>
                //{
                //    Debug.WriteLine($"AhLog: LogLevelChanged: {e.NewLevel}");
                //};
                _defaultLogLevel = LogLevel;
                if (FileName != null) _logFileName = FileName;

                var folder = Environment.SpecialFolder.LocalApplicationData;
                if (Debugger.IsAttached)
                    folder = Environment.SpecialFolder.Desktop;
                var path = Environment.GetFolderPath(folder);
                _logFilePath = Path.Join(path, _logFileName);
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
                _ahLog.LogInformation($"AhLogInit: [{LogLevel}]");
                _ahLog.LogDebug($"Logfile: {_logFilePath}");
                File.SetAttributes(_logFilePath, FileAttributes.Hidden);
            }

            return _ahLog;
        }
    



        public static void Log(string message) => LogDebug(message);
        public static void LogInfo(string message) => LogInformation(message);
        public static void LogMsg(string message) => LogInformation(message);
        public static void LogWarn(string message) => LogWarning(message);

        public static void Log(bool value) => LogDebug(value);
        public static void LogInfo(bool value) => LogInformation(value);
        public static void LogMsg(bool value) => LogInformation(value);
        public static void LogWarn(bool value) => LogWarning(value);


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



        public static void LogTrace(bool value)
        {
            if (value) _LoggingLevel.MinimumLevel = LogEventLevel.Verbose;
            else _LoggingLevel.MinimumLevel = _defaultLogLevel;
        }
        public static void LogDebug(bool value)
        {
            if (value) _LoggingLevel.MinimumLevel = LogEventLevel.Debug;
            else _LoggingLevel.MinimumLevel = _defaultLogLevel;
        }
        public static void LogInformation(bool value)
        {
            if (value) _LoggingLevel.MinimumLevel = LogEventLevel.Information;
            else _LoggingLevel.MinimumLevel = _defaultLogLevel;
        }
        public static void LogWarning(bool value)
        {
            if (value) _LoggingLevel.MinimumLevel = LogEventLevel.Warning;
            else _LoggingLevel.MinimumLevel = _defaultLogLevel;
        }
        public static void LogError(bool value)
        {
            if (value) _LoggingLevel.MinimumLevel = LogEventLevel.Error;
            else _LoggingLevel.MinimumLevel = _defaultLogLevel;
        }
        public static void LogCritical(bool value)
        {
            if (value) _LoggingLevel.MinimumLevel = LogEventLevel.Fatal;
            else _LoggingLevel.MinimumLevel = _defaultLogLevel;
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

