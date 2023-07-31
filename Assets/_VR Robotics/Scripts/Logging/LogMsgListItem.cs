using RosMessageTypes.RclInterfaces;
using TMPro;
using UnityEngine;

class LogMsgListItem : MonoBehaviour
{
    public LoggerSourceUIController UIController { get; set; }

    [SerializeField]
    TextMeshProUGUI m_MsgTextUI;
    [SerializeField]
    TextMeshProUGUI m_MsgTimeUI;

    public LogMsg Message { get => m_LogMsg; }
    private LogMsg m_LogMsg;


    public void SetLogMessage(LogMsg msg)
    {
        m_LogMsg = msg;
        m_MsgTextUI.SetText(m_LogMsg.msg);
        m_MsgTimeUI.SetText(LoggerSourceUIController.GetTimeString(m_LogMsg.stamp.sec));
    }

    public void SetToggleValue(bool value)
    {
        if (value) UIController.SelectMessage(m_LogMsg);
    }
}
