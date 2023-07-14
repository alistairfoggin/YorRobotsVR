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

        if (trackPosition)
        {
            transform.localPosition = -odomPosition;
        }
        if (trackOrientation)
        {
            transform.localRotation = Quaternion.Euler(Vector3.zero);
            if (isMinimap)
            {
                transform.RotateAround(GetComponentInParent<Transform>().position, Vector3.up, -odomOrientation.eulerAngles.y - tfFrame.rotation.eulerAngles.y - rotationFix);
            } else
            {
                transform.RotateAround(Vector3.zero, Vector3.up, -odomOrientation.eulerAngles.y - tfFrame.rotation.eulerAngles.y - rotationFix);

            }
        }
    }

    private void Update()
    {
        
    }
}
