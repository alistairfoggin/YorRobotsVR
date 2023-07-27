using RosMessageTypes.Rosgraph;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.Events;

public class LogUIController : MonoBehaviour
{
    private ROSConnection m_ROSConnection;
    private Dictionary<string, LoggerSource> m_Loggers;

    // TODO: trigger UI updates
    public UnityAction UpdateUI;

    // Start is called before the first frame update
    void Start()
    {
        m_Loggers = new Dictionary<string, LoggerSource>();
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_ROSConnection.Subscribe<LogMsg>("/rosout", AddLog);
    }

    private void AddLog(LogMsg msg)
    {
        LoggerSource logger;
        if (!m_Loggers.TryGetValue(msg.name, out logger))
        {
            logger = new LoggerSource(msg.name);
            m_Loggers.Add(msg.name, logger);
        }

        logger.AddLog(msg);
        UpdateUI();
    }
}
