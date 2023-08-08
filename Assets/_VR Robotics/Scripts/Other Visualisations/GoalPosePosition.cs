using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class GoalPosePosition : MonoBehaviour
{
    private ROSConnection m_ROSConnection;
    private TFSystem m_TFSystem;

    // Start is called before the first frame update
    void Start()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_ROSConnection.Subscribe<PoseStampedMsg>("/goal_pose", UpdatePosition);


        m_TFSystem = TFSystem.GetOrCreateInstance();
    }

    void UpdatePosition(PoseStampedMsg msg)
    {
        Vector3 position = msg.pose.position.From<FLU>();
        transform.localPosition = m_TFSystem.GetTransform(msg.header).TransformPoint(position);
    }
}
