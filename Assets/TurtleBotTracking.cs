using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class TurtleBotTracking : MonoBehaviour
{
    ROSConnection m_RosConnection;
    Rigidbody m_Rigdbody;

    // Start is called before the first frame update
    void Start()
    {
        m_Rigdbody = GetComponent<Rigidbody>();
        m_RosConnection = ROSConnection.GetOrCreateInstance();
        m_RosConnection.Subscribe<OdometryMsg>("/odom", OdomChange);
    }

    void OdomChange(OdometryMsg msg)
    {
        PointMsg pointMsg = msg.pose.pose.position;
        QuaternionMsg quaternionMsg = msg.pose.pose.orientation;
        m_Rigdbody.MovePosition(pointMsg.From<FLU>());
        m_Rigdbody.MoveRotation(quaternionMsg.From<FLU>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
