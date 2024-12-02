using System;
using System.Collections.Generic;

namespace Core.Logger {
    public enum LogLevel {
        TRACE,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL
    }

    public class LogMessage {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string LoggerName { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}