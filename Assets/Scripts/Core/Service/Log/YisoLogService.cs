using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Logger;
using Core.Logger.Interceptor;
using Core.Logger.Security;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

namespace Core.Service.Log {
    public class YisoLogService : IYisoLogService {
        private readonly Settings settings;
        private readonly Dictionary<string, YisoLogger> loggers = new();
        private readonly ConcurrentQueue<LogMessage> globalLogQueue = new();
        private readonly CryptoHelper crypto;
        private bool isProcessing = false;
        private static readonly object lockObject = new();

        private readonly YisoLoggerBehaviour loggerBehaviour;

        public bool IsReady() => true;

        public YisoLogger GetLogger(string name) {
            if (loggers.TryGetValue(name, out var logger)) return logger;

            logger = new YisoLogger(name, settings.enabledLevels.Min());
            loggers[name] = logger;
            return logger;
        }

        public YisoLogger GetLogger<T>() => GetLogger(typeof(T).Name);

        private YisoLogService(Settings settings, YisoLoggerBehaviour loggerBehaviour) {
            crypto = new CryptoHelper(settings.encryptionKey);
            this.settings = settings;
            this.loggerBehaviour = loggerBehaviour;
            this.loggerBehaviour.OnApplicationQuickAction = OnApplicationQuit;

            // this.loggerBehaviour.StartCoroutine(ProcessRemainingLogs());
            // this.loggerBehaviour.StartCoroutine(MaintenanceCoroutine());
        }

        public void EnqueueLog(LogMessage message) {
            if (!settings.enabledLevels.Contains(message.Level)) return;
            if (globalLogQueue.Count >= settings.maxQueueSize) globalLogQueue.TryDequeue(out _);
            globalLogQueue.Enqueue(message);
        }

        private IEnumerator ProcessLogQueueCoroutine() {
            while (true) {
                yield return YieldInstructionCache.WaitForSeconds(settings.processInterval);
                if (isProcessing || globalLogQueue.IsEmpty) continue;
                isProcessing = true;

                try {
                    var batch = new List<LogMessage>();
                    while (batch.Count < settings.batchSize && globalLogQueue.IsEmpty) {
                        if (globalLogQueue.TryDequeue(out var message)) batch.Add(message);
                    }

                    if (batch.Count > 0) {
                        yield return SaveLogsToFile(batch);

                        if (!string.IsNullOrEmpty(settings.logServerUrl)) {
                            yield return SendLogsToServer(batch);
                        }
                    }
                } finally {
                    isProcessing = false;
                }
            }
        }

        private IEnumerator MaintenanceCoroutine() {
            while (true) {
                yield return YieldInstructionCache.WaitForSeconds(settings.maintenanceInterval);

                try {
                    CleanupOldLogs();
                    RotateLogFileIfNeeded();
                } catch (Exception ex) {
                    Debug.LogError($"Log maintenance failed: {ex.Message}");
                }
            }
        }
        
        public void OnDestroy() { }


        private void OnApplicationQuit() {
            loggerBehaviour.StartCoroutine(ProcessRemainingLogs());
        }

        private IEnumerator ProcessRemainingLogs() {
            while (!globalLogQueue.IsEmpty) {
                var batch = new List<LogMessage>();
                while (batch.Count < settings.batchSize && !globalLogQueue.IsEmpty) {
                    if (globalLogQueue.TryDequeue(out var message))
                        batch.Add(message);
                }

                if (batch.Count > 0) yield return SaveLogsToFile(batch);
            }
        }

        private IEnumerator SaveLogsToFile(List<LogMessage> batch) {
            var logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
            var currentLogFile = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyyMMdd}.log");

            try {
                Directory.CreateDirectory(logDirectory);
                var batchContent = SerializeBatch(batch);
                var encrypted = crypto.Encrypt(batchContent);
                
                using var writer = File.AppendText(currentLogFile);
                writer.WriteLine(encrypted);
            } catch (Exception ex) {
                Debug.LogError($"Failed to save logs to file: {ex.Message}");
            }

            yield return null;
        }

        private IEnumerator SendLogsToServer(List<LogMessage> batch) {
            yield break;
        }

        private void CleanupOldLogs() {
            var logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
            if (!Directory.Exists(logDirectory)) return;

            var logFiles = Directory.GetFiles(logDirectory, "log_*.log")
                .OrderByDescending(f => f)
                .Skip(settings.maxLogFiles);

            foreach (var file in logFiles) {
                try {
                    File.Delete(file);
                }
                catch (Exception ex) {
                    Debug.LogError($"Failed to delete old log file {file}: {ex.Message}");
                }
            }
        }

        private void RotateLogFileIfNeeded() {
            var logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
            var currentLogFile = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            
            if (!File.Exists(currentLogFile)) return;

            var fileInfo = new FileInfo(currentLogFile);
            if (fileInfo.Length <= 10 * 1024 * 1024) return;
            var newFileName = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            File.Move(currentLogFile, newFileName);
        }

        private string SerializeBatch(List<LogMessage> batch) {
            return string.Join(Environment.NewLine, batch.Select(msg => {
                var metaDataStr = msg.Metadata != null && msg.Message.Length > 0
                    ? $" | {{{string.Join(", ", msg.Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"))}"
                    : string.Empty;
                return
                    $"{msg.Timestamp:yyyy-MM-dd HH:mm:ss.fff} {msg.Level} {msg.LoggerName} : {msg.Metadata}{metaDataStr}";
            }));
        }

        internal static YisoLogService CreateService(Settings settings, YisoLoggerBehaviour loggerBehaviour) => new(settings, loggerBehaviour);

        [Serializable]
        public class Settings {
            public LogLevel[] enabledLevels = Enum.GetValues(typeof(LogLevel)) as LogLevel[];
            public int maxQueueSize = 1000;
            public int batchSize = 100;
            public float processInterval = 5f;
            public float maintenanceInterval = 300f;
            public int maxLogFiles = 30;
            public string logServerUrl;
            public string encryptionKey = "q%7a6yP/Zy8Z*-0";
        }
    }
}