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

    // Start is called before the first frame update
    void Start()
    {
        m_RosConnection = ROSConnection.GetOrCreateInstance();
        m_RosConnection.Subscribe<OdometryMsg>("/odom", OdomChange);
    }

    void OdomChange(OdometryMsg msg)
    {
        PointMsg pointMsg = msg.pose.pose.position;
        QuaternionMsg quaternionMsg = msg.pose.pose.orientation;

        Vector3 odomPosition = pointMsg.From<FLU>();
        Quaternion odomOrientation = quaternionMsg.From<FLU>();

        transform.localPosition = -odomPosition;
        //transform.localRotation = Quaternion.Euler(-odomOrientation.eulerAngles);
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.RotateAround(Vector3.zero, Vector3.up, -odomOrientation.eulerAngles.y - 90);
    }
}
