using RosMessageTypes.Rosgraph;
using System.Collections.Generic;

class LoggerSource
{
    public enum LogLevel : sbyte
    {
        DEBUG = 10,
        INFO = 10,
        WARN = 10,
        ERROR = 10,
        FATAL = 10,
    }
    string m_Name;

    Dictionary<LogLevel, Queue<LogMsg>> m_LogsByLevel;

    public LoggerSource(string name)
    {
        m_Name = name;

        m_LogsByLevel.Add(LogLevel.DEBUG, new Queue<LogMsg>(8));
        m_LogsByLevel.Add(LogLevel.INFO, new Queue<LogMsg>(8));
        m_LogsByLevel.Add(LogLevel.WARN, new Queue<LogMsg>(8));
        m_LogsByLevel.Add(LogLevel.ERROR, new Queue<LogMsg>(8));
        m_LogsByLevel.Add(LogLevel.FATAL, new Queue<LogMsg>(8));
    }

    public bool AddLog(LogMsg msg)
    {
        if (msg.name != m_Name) return false;
        Queue<LogMsg> queue;
        if (!m_LogsByLevel.TryGetValue((LogLevel)msg.level, out queue)) {
            return false;
        }
        // Limit number of logs maintained
        if (queue.Count == 8)
        {
            queue.Dequeue();
        }
        queue.Enqueue(msg);
        return true;
    }

    public Queue<LogMsg> GetLogsForLevel(LogLevel logLevel)
    {
        Queue<LogMsg> queue;
        m_LogsByLevel.TryGetValue(logLevel, out queue);
        return queue;
    }
}
