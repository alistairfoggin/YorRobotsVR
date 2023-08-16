using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class TurtlebotTracking : MonoBehaviour
{
    private ROSConnection m_RosConnection;
    private TFSystem m_TFSystem;

    [SerializeField]
    bool trackPosition = true;
    [SerializeField]
    bool trackOrientation = true;

    float m_LastTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_RosConnection = ROSConnection.GetOrCreateInstance();
        m_RosConnection.Subscribe<OdometryMsg>("/odom", OdomChange);

        m_TFSystem = TFSystem.GetOrCreateInstance();
    }

    void OdomChange(OdometryMsg msg)
    {
        if (Time.time <= m_LastTime) return;

        PointMsg pointMsg = msg.pose.pose.position;
        QuaternionMsg quaternionMsg = msg.pose.pose.orientation;

        Vector3 odomPosition = pointMsg.From<FLU>();
        Quaternion odomOrientation = quaternionMsg.From<FLU>();

        TFFrame tfFrame = m_TFSystem.GetTransform(msg.header);
        odomPosition = tfFrame.TransformPoint(odomPosition);
        odomPosition.y = 0;

        if (trackPosition)
        {
            transform.localPosition = odomPosition;
        }
        if (trackOrientation)
        {
            transform.localRotation = Quaternion.Euler(odomOrientation.eulerAngles + tfFrame.rotation.eulerAngles);
        }

        m_LastTime = Time.time;
    }
}
