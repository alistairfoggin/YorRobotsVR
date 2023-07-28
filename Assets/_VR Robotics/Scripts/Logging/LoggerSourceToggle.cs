using RosMessageTypes.RclInterfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

class LoggerSourceToggle : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_LoggerNameLabel;
    [SerializeField]
    private TextMeshProUGUI m_LogCountLabel;
    [SerializeField]
    private Toggle m_Toggle;
    private LoggerSource m_LoggerSource;
    private LoggersUIController m_LoggersUIController;

    public void SetLogger(LoggersUIController loggersUIController, LoggerSource loggerSource, ToggleGroup toggleGroup)
    {
        m_LoggersUIController = loggersUIController;
        m_LoggerSource = loggerSource;
        m_Toggle.group = toggleGroup;

        m_LoggerNameLabel.SetText(m_LoggerSource.Name);
    }

    public void UpdateLabels()
    {
        m_LogCountLabel.SetText(m_LoggerSource.NumLogs.ToString());
    }

    public void SetToggleValue(bool value)
    {
        m_LoggersUIController.SetToggle(m_LoggerSource, value);
    }
}
