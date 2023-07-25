using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RobotNavigationInteractable : XRBaseInteractable
{
    ROSConnection m_ROSConnection;
    ROSTime m_ROSTime;
    TFSystem m_TFSystem;
    public string topicName = "/goal_pose";

    [SerializeField]
    Transform mapCentreTransform;

    private void Start()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_ROSConnection.RegisterPublisher<PoseStampedMsg>(topicName);

        m_ROSTime = ROSTime.GetOrCreateInstance();

        m_TFSystem = TFSystem.GetOrCreateInstance();
    }

    public void GoToPoint(Vector3 goal)
    {
        PoseStampedMsg msg = new PoseStampedMsg(
            new HeaderMsg(new TimeMsg(), "map"),
            new PoseMsg(goal.To<FLU>(), new QuaternionMsg()));

        m_ROSConnection.Publish(topicName, msg);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRRayInteractor)
        {
            XRRayInteractor rayInteractor = (XRRayInteractor)args.interactorObject;
            Vector3 hitPosition;
            Vector3 hitNormal;
            rayInteractor.TryGetHitInfo(out hitPosition, out hitNormal, out _, out _);

            if (Vector3.Dot(hitNormal.normalized, transform.up) < 0.95f)
            {
                return;
            }

            Vector3 destination;
            if (mapCentreTransform != null)
            {
                destination = mapCentreTransform.InverseTransformPoint(hitPosition);
                destination.Scale(mapCentreTransform.GetComponentInParent<Transform>().localScale);
            }
            else
            {
                destination = transform.InverseTransformPoint(hitPosition);
                destination.Scale(transform.localScale);
            }

            if (m_ROSTime.LatestTimeMsg != null)
            {
                destination = m_TFSystem.GetTransform("map", m_ROSTime.LatestTimeMsg).InverseTransformPoint(destination);
            }
            destination.y = 0;
            GoToPoint(destination);
        }
    }
}
