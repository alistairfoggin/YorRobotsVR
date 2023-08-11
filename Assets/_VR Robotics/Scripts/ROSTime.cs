using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Rosgraph;
using RosMessageTypes.BuiltinInterfaces;

public class ROSTime
{
    private static ROSTime instance;

    private TimeMsg m_TimeMsg = new TimeMsg();

    private ROSTime()
    {
        var rosConnection = ROSConnection.GetOrCreateInstance();
        rosConnection.Subscribe<ClockMsg>("/clock", UpdateTime); // Does not necessarily work if the topic has QoS settings incompatible with ROS TCP Endpoint
    }

    private void UpdateTime(ClockMsg msg)
    {
        m_TimeMsg = msg.clock;
    }

    public static ROSTime GetOrCreateInstance()
    {
        if (instance == null)
        {
            instance = new ROSTime();
        }
        return instance;
    }

    /// <summary>
    /// Returns the latest ROS time. null if no clock message has been received yet.
    /// </summary>
    public TimeMsg LatestTimeMsg => m_TimeMsg;
}
