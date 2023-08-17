using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class InverseMapTracking : MonoBehaviour
{
    private ROSConnection m_RosConnection;
    private TFSystem m_TFSystem;

    [SerializeField]
    bool trackOrientation = true;
    [SerializeField]
    bool trackPosition = true;
    [SerializeField]
    float rotationFix = 0.0f;
    [SerializeField]
    bool isMinimap = true;

    void Start()
    {
        m_RosConnection = ROSConnection.GetOrCreateInstance();
        m_RosConnection.Subscribe<OdometryMsg>("/odom", OdomChange);
        m_TFSystem = TFSystem.GetOrCreateInstance();
    }

    void OdomChange(OdometryMsg msg)
    {
        PointMsg pointMsg = msg.pose.pose.position;
        QuaternionMsg quaternionMsg = msg.pose.pose.orientation;

        // Convert ROS local coordinates to Unity coordinates local coordinates
        Vector3 odomPosition = pointMsg.From<FLU>();
        Quaternion odomOrientation = quaternionMsg.From<FLU>();

        // Transform position to be based around ROS (0, 0)
        TFFrame tfFrame = m_TFSystem.GetTransform(msg.header);
        odomPosition = tfFrame.TransformPoint(odomPosition);
        odomPosition.y = 0;

        // Move map based on the robot position
        if (trackPosition)
        {
            transform.localPosition = -odomPosition;
        }
        // Rotate the map based on the robot orientation
        if (trackOrientation)
        {
            transform.localRotation = Quaternion.Euler(Vector3.zero);
            // Find the point to rotate around
            if (isMinimap)
            {
                transform.RotateAround(GetComponentInParent<Transform>().position, Vector3.up, -odomOrientation.eulerAngles.y - tfFrame.rotation.eulerAngles.y - rotationFix);
            }
            else
            {
                transform.RotateAround(Vector3.zero, Vector3.up, -odomOrientation.eulerAngles.y - tfFrame.rotation.eulerAngles.y - rotationFix);

            }
        }
    }
}
