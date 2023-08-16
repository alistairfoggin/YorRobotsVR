using RosMessageTypes.RclInterfaces;
using System;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoggersUIController : MonoBehaviour
{
    [SerializeField]
    private LoggerSourceToggle m_TogglePrefab;
    [SerializeField]
    private LoggerSourceUIController m_LoggerSourceController;

    private ToggleGroup m_ToggleGroup;
    private ROSConnection m_ROSConnection;
    private Dictionary<string, LoggerSource> m_Loggers;
    private Dictionary<string, LoggerSourceToggle> m_LoggerToggles;


    // Start is called before the first frame update
    void Start()
    {
        m_ToggleGroup = GetComponent<ToggleGroup>();
        m_Loggers = new Dictionary<string, LoggerSource>();
        m_LoggerToggles = new Dictionary<string, LoggerSourceToggle>();
        m_LoggerSourceController.gameObject.SetActive(false);

        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_ROSConnection.Subscribe<LogMsg>("/rosout", AddLog);
    }

    private void AddLog(LogMsg msg)
    {
        LoggerSource logger;
        LoggerSourceToggle toggle;
        // Add toggle for the source log name if it is not already there
        if (!m_Loggers.TryGetValue(msg.name, out logger))
        {
            logger = new LoggerSource(msg.name);
            m_Loggers.Add(msg.name, logger);
            toggle = Instantiate(m_TogglePrefab);
            toggle.transform.SetParent(transform, false);
            toggle.SetLogger(this, logger, m_ToggleGroup);
            m_LoggerToggles.Add(msg.name, toggle);
        }
        logger.AddLog(msg);

        // Update UI after update
        if (m_LoggerToggles.TryGetValue(msg.name, out toggle))
        {
            toggle.UpdateLabels();
        }
        if (m_LoggerSourceController.gameObject.activeInHierarchy && m_LoggerSourceController.GetSource() == logger)
        {
            m_LoggerSourceController.UpdateUI();
        }
    }

    public void SetToggle(LoggerSource source, bool value)
    {
        if (!value && !m_ToggleGroup.AnyTogglesOn())
        {
            m_LoggerSourceController.gameObject.SetActive(false);
        }
        if (value)
        {
            m_LoggerSourceController.gameObject.SetActive(true);
            m_LoggerSourceController.SetSource(source);
        }
    }
}
