using RosMessageTypes.Nav;
using System.Linq;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
class RobotPlan : MonoBehaviour
{
    ROSConnection m_ROSConnection;
    TFSystem m_TFSystem;
    LineRenderer m_LineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_TFSystem = TFSystem.GetOrCreateInstance();
        m_LineRenderer = GetComponent<LineRenderer>();

        m_ROSConnection.Subscribe<PathMsg>("/plan", UpdatePlan);
    }

    private void UpdatePlan(PathMsg msg)
    {
        TFFrame tf = m_TFSystem.GetTransform(msg.header);
        // Map array of pose messages to be an array of Unity Vectors
        Vector3[] unityPoints = msg.poses.Select(poseStamped => tf.TransformPoint(poseStamped.pose.position.From<FLU>())).ToArray();
        m_LineRenderer.positionCount = unityPoints.Length;
        m_LineRenderer.SetPositions(unityPoints);
    }
}
