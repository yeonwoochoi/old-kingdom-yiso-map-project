using System;
using System.Collections.Generic;
using Core.Service.Log;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Logger.Interceptor {
    public class UnityLogInterceptor : ILogHandler {
        private readonly IYisoLogService logService;
        private readonly YisoLogger logger;
        private readonly ILogHandler defaultLogHandler;
        private static UnityLogInterceptor instance;

        public static void Initialize(IYisoLogService logService) {
            if (instance == null) {
                instance = new UnityLogInterceptor(logService);
                Debug.unityLogger.logHandler = instance;
            }
        }

        private UnityLogInterceptor(IYisoLogService logService) {
            this.logService = logService;
            logger = logService.GetLogger("Unity");
            defaultLogHandler = Debug.unityLogger.logHandler;
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) {
            var message = string.Format(format, args);
            HandleLog(logType, context, message);
            
            defaultLogHandler.LogFormat(logType, context, format, args);
        }
        
        public void LogException(Exception exception, Object context) {
            var metadata = new Dictionary<string, object>() {
                ["ExceptionType"] = exception.GetType().Name,
                ["StackTrace"] = exception.StackTrace
            };
            
            logger.Error($"Exception: {exception.Message}", metadata, context);
            defaultLogHandler.LogException(exception, context);
        }

        private void HandleLog(LogType logType, Object context, string message) {
            var metadata = new Dictionary<string, object>();
            if (context != null) {
                metadata["UnityContext"] = context.GetType().Name;
                metadata["ContextName"] = context.name;
            }

            switch (logType) {
                case LogType.Error:
                    logger.Error(message, metadata, context);
                    break;
                case LogType.Assert:
                    logger.Error($"Assertion failed: {message}", metadata, context);
                    break;
                case LogType.Warning:
                    logger.Warn(message, metadata, context);
                    break;
                case LogType.Log:
                    logger.Info(message, metadata, context);
                    break;
                case LogType.Exception:
                    logger.Fatal(message, metadata, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }
    }
}