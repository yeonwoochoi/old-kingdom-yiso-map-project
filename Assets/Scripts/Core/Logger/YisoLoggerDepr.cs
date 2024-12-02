using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Core.Behaviour;
using Core.Service;
using Core.Service.Log;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;
using Utils;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Core.Logger {
    public class YisoLoggerDepr : RunIBehaviour {
        private Settings settings;
        
        private static List<string> logMessages = new();
        
        public void SetSettings(Settings settings) {
            this.settings = settings;
        }

        protected override void Awake() {
            base.Awake();
            // YisoServiceProvider.Instance.Get<IYisoLogService>().SetLogger(this);
        }

        protected override void OnEnable() {
            base.OnEnable();
            Application.logMessageReceived += HandleLog;
        }

        protected override void OnDisable() {
            base.OnDisable();
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type) {
            var callerClassName = GetCallerClassName();
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{type}] [{callerClassName}] {logString}";
            logMessages.Add(logMessage);
        }

        public static void Log(string message) {
#if LOGGING_ENABLED
        LogInternal(LogLevel.INFO, message, null);
#endif
        }

        public static void Log(string message, Object context) {
#if LOGGING_ENABLED
        LogInternal(LogLevel.INFO, message, context);
#endif
        }

        public static void LogWarning(string message) {
#if LOGGING_ENABLED
        LogInternal(LogLevel.WARN, message, null);
#endif
        }

        public static void LogWarning(string message, Object context) {
#if LOGGING_ENABLED
        LogInternal(LogLevel.WARN, message, context);
#endif
        }

        public static void LogError(string message) {
#if LOGGING_ENABLED
        LogInternal(LogLevel.ERROR, message, null);
#endif
        }

        public static void LogError(string message, Object context) {
#if LOGGING_ENABLED
        LogInternal(LogLevel.ERROR, message, context);
#endif
        }

        private static void LogInternal(LogLevel level, string message, Object context) {
            var callerClassName = GetCallerClassName();
            var logMessage = FormatLog(level, message, callerClassName);

            switch (level) {
                case LogLevel.INFO:
                    if (context != null) UnityEngine.Debug.Log(logMessage, context);
                    else UnityEngine.Debug.Log(logMessage);
                    break;
                case LogLevel.WARN:
                    if (context != null) UnityEngine.Debug.LogWarning(logMessage, context);
                    else UnityEngine.Debug.LogWarning(logMessage);
                    break;
                case LogLevel.ERROR:
                    if (context != null) UnityEngine.Debug.LogError(logMessage, context);
                    else UnityEngine.Debug.LogError(logMessage);
                    break;
                case LogLevel.DEBUG:
                    if (context != null) UnityEngine.Debug.Log(logMessage, context);
                    else UnityEngine.Debug.Log(logMessage);
                    break;
            }

            logMessages.Add(logMessage);
        }

        private static string FormatLog(LogLevel level, string message, string callerClassName) {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return $"{timestamp} [{level}] [{callerClassName}] {message}";
        }

        private static string GetCallerClassName() {
            return "Unknown";
        }
        
        private static string GetCallerClsName() {
            try {
                var reflectedType = new StackTrace().GetFrame(1).GetMethod().ReflectedType;
                if (reflectedType == null) return "";
                var callerObjectName = reflectedType.Name;
                return callerObjectName;
            } catch (NullReferenceException e) {
                return "";
            }
        }

        public void Maintain() {
            // var files = Dictionary
        }

        public void Dispose() {
            
        }

        public enum LogLevel {
            INFO,
            WARN,
            ERROR,
            DEBUG
        }

        [Serializable]
        public class Settings {
            public LogLevel[] enabledLevels = Enum.GetValues(typeof(LogLevel)) as LogLevel[];
            public int maxQueueSize = 1000;
            public int batchSize = 100;
            public float processInterval = 5f;
            public float maintenanceInterval = 300f;
            public int maxLogFiles = 30;
            public string logServerUrl;
        }
    }
}