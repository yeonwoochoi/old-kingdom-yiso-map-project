using System.Collections;

namespace Core.Logger.Appender {
    public interface ILogAppender {
        void AppendLog(LogMessage message);
        IEnumerator DOAppendLog(LogMessage message);
    }
}