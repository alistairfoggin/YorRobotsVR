using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Rosgraph;
using RosMessageTypes.BuiltinInterfaces;

public class RobotNavigationInteractable : XRBaseInteractable
{
    ROSConnection m_ROSConnection;
    ROSTime m_ROSTime;
    TFSystem m_TFSystem;
    public string topicName = "/goal_pose";
    private void Start()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_ROSConnection.RegisterPublisher<PoseStampedMsg>(topicName);

        m_ROSTime = ROSTime.GetOrCreateInstance();

        m_TFSystem = TFSystem.GetOrCreateInstance();
        GoToPoint(new Vector3(0.2f, 0, 0));
    }

    public void GoToPoint(Vector3 goal)
    {
        PoseStampedMsg msg = new PoseStampedMsg(
            new HeaderMsg(new TimeMsg(), "map"),
            new PoseMsg(goal.To<FLU>(), new QuaternionMsg()));

        m_ROSConnection.Publish(topicName, msg);
    }

    protected override void OnActivated(ActivateEventArgs args)
    {
        if (args.interactorObject is XRRayInteractor)
        {
            XRRayInteractor rayInteractor = (XRRayInteractor)args.interactorObject;
            Vector3 hitPosition;
            Vector3 hitNormal;
            rayInteractor.TryGetHitInfo(out hitPosition, out hitNormal, out _, out _);

            Vector3 destination = GetComponentInParent<Transform>().InverseTransformPoint(hitPosition);

            if (m_ROSTime.LatestTimeMsg != null)
            {
                destination = m_TFSystem.GetTransform("map", m_ROSTime.LatestTimeMsg).InverseTransformPoint(destination);
            }

            destination = Quaternion.Euler(0, 90, 0) * destination;
            
            // destination.Scale(transform.localScale);
            //destination = Quaternion.Euler(0, 90, 0) * destination;
            GoToPoint(destination);
        }
    }

}
