using System;
using System.Collections.Generic;

namespace Core.Exceptions {
    public class YisoException : Exception {
        public ErrorInfo ErrorInfo { get; private set; }

        public YisoException(string message, ErrorInfo errorInfo) : base(message) {
            ErrorInfo = errorInfo;
        }

        public YisoException(string message, string errorCode, ErrorPriority priority = ErrorPriority.ERROR) : this(message, new ErrorInfo(errorCode, message, priority)) { }
    }

    public class ErrorInfo {
        public string ErrorCode { get; private set; }
        public string Message { get; private set; }
        public ErrorPriority Priority { get; private set; }
        public DateTime Timestamp { get; private set; }
        public Dictionary<string, string> Context { get; private set; }

        public ErrorInfo(string errorCode, string message, ErrorPriority priority) {
            ErrorCode = errorCode;
            Message = message;
            Priority = priority;
            Timestamp = DateTime.UtcNow;
            Context = new Dictionary<string, string>();
        }

        public void AddContext(string key, string value) {
            Context[key] = value;
        }
    }

    public enum ErrorPriority {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        CRITICAL
    }
}