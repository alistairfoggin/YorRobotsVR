using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class InverseMapTracking : MonoBehaviour
{
    private ROSConnection m_RosConnection;
    private TFSystem m_TFSystem;

    // Start is called before the first frame update
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

        Vector3 odomPosition = pointMsg.From<FLU>();
        Quaternion odomOrientation = quaternionMsg.From<FLU>();

        TFFrame tfFrame = m_TFSystem.GetTransform(msg.header);
        odomPosition = tfFrame.TransformPoint(odomPosition);
        odomPosition.y = 0;

        transform.localPosition = -odomPosition;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.RotateAround(Vector3.zero, Vector3.up, -odomOrientation.eulerAngles.y - tfFrame.rotation.eulerAngles.y - 90);
    }
}
