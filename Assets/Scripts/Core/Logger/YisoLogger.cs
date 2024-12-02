using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Core.Logger.Appender;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Core.Logger {
    public class YisoLogger {
        private readonly string name;
        private readonly LogLevel minimumLevel;
        private readonly int processId;

        private readonly Dictionary<LogLevel, Color> logColors = new Dictionary<LogLevel, Color>() {
            { LogLevel.TRACE, Color.gray },
            { LogLevel.DEBUG, Color.white },
            { LogLevel.INFO, Color.green },
            { LogLevel.WARN, Color.yellow },
            { LogLevel.ERROR, Color.red },
            { LogLevel.FATAL, new Color(0.5f, 0, 0) }
        };

        public YisoLogger(string name, LogLevel minimumLevel) {
            this.name = name;
            this.minimumLevel = minimumLevel;
            processId = Process.GetCurrentProcess().Id;
        }


        private string GetThreadName() {
            var managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (Application.isPlaying && managedThreadId == 1) return "Thread-Main";
            return $"Thread-{managedThreadId}";
        }

        private string FormatLogMessage(LogLevel level, string message, Dictionary<string, object> metadata = null) {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            var levelPadded = level.ToString().PadRight(5);
            var threadName = GetThreadName();
            var metadataStr = metadata is { Count: > 0 }
                ? $" | {{{string.Join(", ", metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"))}"
                : string.Empty;

            return $"{timestamp} {levelPadded} {processId} --- [{threadName}] {name} : {message}{metadataStr}";
        }

        private void SendToUnityConsole(LogLevel level, string formattedMessage, UnityEngine.Object context = null) {
            switch (level) {
                case LogLevel.TRACE:
                case LogLevel.DEBUG:
                case LogLevel.INFO:
                    UnityEngine.Debug.Log(formattedMessage, context);
                    break;
                case LogLevel.WARN:
                    UnityEngine.Debug.LogWarning(formattedMessage, context);
                    break;
                case LogLevel.ERROR:
                case LogLevel.FATAL:
                    UnityEngine.Debug.LogError(formattedMessage, context);
                    break;
            }
        }

        private void LogInternal(LogLevel level, string message, Dictionary<string, object> metadata = null,
            UnityEngine.Object context = null) {
            if (level < minimumLevel) return;

            var logMessage = new LogMessage {
                Timestamp = DateTime.UtcNow,
                Level = level,
                LoggerName = name,
                Message = message,
                StackTrace = level >= LogLevel.ERROR ? Environment.StackTrace : null,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            var formattedMessage = FormatLogMessage(level, message, metadata);
            SendToUnityConsole(level, formattedMessage, context);
        }

        public void Trace(string message, UnityEngine.Object context = null) =>
            LogInternal(LogLevel.TRACE, message, null, context);
        
        public void Debug(string message, UnityEngine.Object context = null) => 
            LogInternal(LogLevel.DEBUG, message, null, context);

        public void Info(string message, UnityEngine.Object context = null) => 
            LogInternal(LogLevel.INFO, message, null, context);

        public void Warn(string message, UnityEngine.Object context = null) => 
            LogInternal(LogLevel.WARN, message, null, context);

        public void Error(string message, UnityEngine.Object context = null) => 
            LogInternal(LogLevel.ERROR, message, null, context);

        public void Fatal(string message, UnityEngine.Object context = null) => 
            LogInternal(LogLevel.FATAL, message, null, context);
        
        public void Trace(string message, Dictionary<string, object> metadata, UnityEngine.Object context = null) => 
            LogInternal(LogLevel.TRACE, message, metadata, context);

        public void Debug(string message, Dictionary<string, object> metadata, UnityEngine.Object context = null) => 
            LogInternal(LogLevel.DEBUG, message, metadata, context);

        public void Info(string message, Dictionary<string, object> metadata, UnityEngine.Object context = null) => 
            LogInternal(LogLevel.INFO, message, metadata, context);

        public void Warn(string message, Dictionary<string, object> metadata, UnityEngine.Object context = null) => 
            LogInternal(LogLevel.WARN, message, metadata, context);

        public void Error(string message, Dictionary<string, object> metadata, UnityEngine.Object context = null) => 
            LogInternal(LogLevel.ERROR, message, metadata, context);

        public void Fatal(string message, Dictionary<string, object> metadata, UnityEngine.Object context = null) => 
            LogInternal(LogLevel.FATAL, message, metadata, context);

        public void Exception(Exception ex, UnityEngine.Object context = null) {
            var metadata = new Dictionary<string, object> {
                ["ExceptionType"] = ex.GetType().Name
            };

            var message = $"Exception occurred: {ex.Message}";
            LogInternal(LogLevel.ERROR, message, metadata, context);

            if (!string.IsNullOrEmpty(ex.StackTrace)) {
                LogInternal(LogLevel.DEBUG, "Stack trace:", null, context);
                foreach (var line in ex.StackTrace.Split("\n")) {
                    if (!string.IsNullOrWhiteSpace(line)) {
                        LogInternal(LogLevel.DEBUG, line.Trim(), null, context);
                    }
                }
            }
        }

        public void Format(LogLevel level, string format, UnityEngine.Object context, params object[] args) {
            var message = string.Format(format, args);
            LogInternal(level, message, null, context);
        }

        public void Assert(bool condition, string message, UnityEngine.Object context = null) {
            if (context) return;
            LogInternal(LogLevel.ERROR, $"Assertion failed: {message}", null, context);
            UnityEngine.Debug.Assert(condition, message, context);
        }
    }
}