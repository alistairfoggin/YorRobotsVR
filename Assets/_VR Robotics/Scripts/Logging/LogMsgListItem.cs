using System;
using System.Collections.Generic;
using RosMessageTypes.RclInterfaces;
using UnityEngine;
using TMPro;

class LogMsgListItem : MonoBehaviour
{
    public LoggerSourceUIController LoggerSourceUIController { get; set; }

    [SerializeField]
    TextMeshProUGUI m_MsgTextUI;
    [SerializeField]
    TextMeshProUGUI m_MsgTimeUI;

    private LogMsg m_LogMsg;


    public void SetLogMessage(LogMsg msg)
    {
        m_LogMsg = msg;
        m_MsgTextUI.SetText(m_LogMsg.msg);
        m_MsgTimeUI.SetText(GetTimeString(m_LogMsg.stamp.sec));
    }

    public void SetToggleValue(bool value)
    {
        LoggerSourceUIController.SelectMessage(m_LogMsg);
    }

    private string GetTimeString(int timeInSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(timeInSeconds);

        return time.ToString(@"hh\:mm\:ss\:fff");
    }
}
