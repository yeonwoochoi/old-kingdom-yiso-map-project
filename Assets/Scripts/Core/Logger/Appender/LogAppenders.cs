using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.Logger.Appender {
    public class LogFileAppender : ILogAppender {
        private readonly string filePath;
        private readonly object lockObject = new();
        private readonly Queue<LogMessage> logQueue;
        private bool isProcessing;

        private readonly Action<IEnumerator> startCoroutineAction;

        public LogFileAppender(string filePath, Action<IEnumerator> startCoroutineAction) {
            this.filePath = filePath;
            logQueue = new Queue<LogMessage>();
            isProcessing = false;
            this.startCoroutineAction = startCoroutineAction;
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        }
        
        public void AppendLog(LogMessage message) {
            lock (lockObject) {
                logQueue.Enqueue(message);
                if (!isProcessing) {
                    isProcessing = true;
                    startCoroutineAction(DOProcessLogQueue());
                }
            }
        }

        private IEnumerator DOProcessLogQueue() {
            while (true) {
                LogMessage message = null;
                if (logQueue.Count > 0) message = logQueue.Dequeue();
                else {
                    isProcessing = false;
                    yield break;
                }

                var logEntry = $"[{message.Timestamp:yyyy-MM-dd HH:mm:ss:fff}] [{message.Level}] [{message.LoggerName}] {message.Message}";
                if (!string.IsNullOrEmpty(message.StackTrace))
                    logEntry += $"\n{message.StackTrace}";

                if (message.Metadata.Any())
                    logEntry += $"\nMetadata: {string.Join(", ", message.Metadata.Select(x => $"{x.Key}=>{x.Value}"))}";

                using (var writer = File.AppendText(filePath)) {
                    writer.WriteLine(logEntry);
                }

                yield return null;
            }
        }

        public IEnumerator DOAppendLog(LogMessage message) {
            AppendLog(message);
            yield return null;
        }
    }

    public class LogServerAppender : ILogAppender {
        public void AppendLog(LogMessage message) {
            
        }

        public IEnumerator DOAppendLog(LogMessage message) {
            yield return null;
        }
    }
}