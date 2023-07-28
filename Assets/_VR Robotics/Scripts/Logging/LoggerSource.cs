using RosMessageTypes.RclInterfaces;
using System.Collections.Generic;
using UnityEngine;

public class LoggerSource
{
    public enum LogLevel : sbyte
    {
        DEBUG = 10,
        INFO = 20,
        WARN = 30,
        ERROR = 40,
        FATAL = 50,
    }
    public uint NumLogs { get; private set; }
    public string Name { get; private set; }

    private Dictionary<LogLevel, Queue<LogMsg>> m_LogsByLevel;

    public LoggerSource(string name)
    {
        Name = name;

        m_LogsByLevel = new Dictionary<LogLevel, Queue<LogMsg>>();
        m_LogsByLevel.Add(LogLevel.DEBUG, new Queue<LogMsg>(5));
        m_LogsByLevel.Add(LogLevel.INFO, new Queue<LogMsg>(5));
        m_LogsByLevel.Add(LogLevel.WARN, new Queue<LogMsg>(5));
        m_LogsByLevel.Add(LogLevel.ERROR, new Queue<LogMsg>(5));
        m_LogsByLevel.Add(LogLevel.FATAL, new Queue<LogMsg>(5));
    }

    public bool AddLog(LogMsg msg)
    {
        if (msg.name != Name) return false;
        Queue<LogMsg> queue;
        if (!m_LogsByLevel.TryGetValue((LogLevel)msg.level, out queue))
        {
            return false;
        }
        // Limit number of logs maintained
        if (queue.Count == 5)
        {
            queue.Dequeue();
        }
        queue.Enqueue(msg);
        NumLogs++;
        return true;
    }

    public Queue<LogMsg> GetLogsForLevel(LogLevel logLevel)
    {
        Queue<LogMsg> queue;
        m_LogsByLevel.TryGetValue(logLevel, out queue);
        return queue;
    }
}
