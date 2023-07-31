using RosMessageTypes.RclInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

class LoggerSourceUIController : MonoBehaviour
{
    [SerializeField]
    private ToggleGroup m_MessageToggleGroup;
    [SerializeField]
    private List<LogMsgListItem> m_LogMsgListItems;

    [SerializeField]
    private TextMeshProUGUI m_MsgLabel;
    [SerializeField]
    private TextMeshProUGUI m_SourceLabel;

    private LoggerSource m_LoggerSource;
    private LoggerSource.LogLevel m_LogLevel = LoggerSource.LogLevel.INFO;
    private LogMsg m_LogMsg;

    private void Start()
    {
        foreach (LogMsgListItem listItem in m_LogMsgListItems)
        {
            listItem.UIController = this;
        }
        m_MsgLabel.SetText("");
        m_SourceLabel.SetText("");
    }

    public void SetSource(LoggerSource source)
    {
        m_LoggerSource = source;
        m_MsgLabel.SetText("");
        m_SourceLabel.SetText("");
        UpdateUI();
    }
    public LoggerSource GetSource()
    {
        return m_LoggerSource;
    }

    public void UpdateUI()
    {
        LogMsg[] logMsgs = m_LoggerSource.GetLogsForLevel(m_LogLevel).ToArray();
        for (int i = 0; i < m_LogMsgListItems.Count; i++)
        {
            LogMsgListItem listItem = m_LogMsgListItems[i];
            if (i >= logMsgs.Length)
            {
                listItem.gameObject.SetActive(false);
                listItem.GetComponent<Toggle>().isOn = false;
            }
            else
            {
                listItem.gameObject.SetActive(true);
                listItem.SetLogMessage(logMsgs[i]);
                listItem.GetComponent<Toggle>().isOn = listItem.Message == m_LogMsg;
            }
        }

        if (m_MessageToggleGroup.AnyTogglesOn() && m_LogMsg != null)
        {
            // show message
            var sb = new StringBuilder();
            sb.AppendFormat("{0} - {1}", GetTimeString(m_LogMsg.stamp.sec), m_LogMsg.msg);
            m_MsgLabel.SetText(sb);
            sb.Clear();
            string[] filePath = m_LogMsg.file.Split('/');
            sb.AppendFormat("[{0}]: {1} (line {2})", m_LogMsg.name, filePath[filePath.Length - 1], m_LogMsg.line);
            m_SourceLabel.SetText(sb);
        }
    }

    public void SelectMessage(LogMsg msg)
    {
        m_LogMsg = msg;
        m_MsgLabel.SetText("");
        m_SourceLabel.SetText("");
        UpdateUI();
    }

    public static string GetTimeString(int timeInSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(timeInSeconds);

        return time.ToString(@"hh\:mm\:ss");
    }

    public void SetDebug(bool value)
    {
        if (value) SetLogLevel(LoggerSource.LogLevel.DEBUG);
    }
    public void SetInfo(bool value)
    {
        if (value) SetLogLevel(LoggerSource.LogLevel.INFO);
    }
    public void SetWarn(bool value)
    {
        if (value) SetLogLevel(LoggerSource.LogLevel.WARN);
    }
    public void SetError(bool value)
    {
        if (value) SetLogLevel(LoggerSource.LogLevel.ERROR);
    }
    public void SetFatal(bool value)
    {
        if (value) SetLogLevel(LoggerSource.LogLevel.FATAL);
    }
    private void SetLogLevel(LoggerSource.LogLevel logLevel)
    {
        m_LogLevel = logLevel;
        UpdateUI();
    }
}
