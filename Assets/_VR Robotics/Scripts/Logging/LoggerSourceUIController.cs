using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RosMessageTypes.RclInterfaces;

class LoggerSourceUIController : MonoBehaviour
{
    [SerializeField]
    private List<LogMsgListItem> m_LogMsgListItems;

    private LoggerSource m_LoggerSource;
    private LoggerSource.LogLevel m_LogLevel;

    public void SetLogLevel(LoggerSource.LogLevel logLevel)
    {
        m_LogLevel = logLevel;
        UpdateUI();
    }

    public void SetSource(LoggerSource source)
    {
        m_LoggerSource = source;
        UpdateUI();
    }

    private void UpdateUI()
    {
        LogMsg[] logMsgs = m_LoggerSource.GetLogsForLevel(m_LogLevel).ToArray();

    }

    public void SelectMessage(LogMsg m_LogMsg)
    {
        throw new NotImplementedException();
    }
}
