using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class RobotNavigationInteractable : XRBaseInteractable
{
    ROSConnection ros;
    public string topicName = "/goal_pose";
    private void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseStampedMsg>(topicName);
        GoToPoint(new Vector3(0.2f, 0, 0));
    }

    public void GoToPoint(Vector3 goal)
    {
        PoseStampedMsg msg = new PoseStampedMsg(
            new HeaderMsg(new TimeMsg(), "map"),
            new PoseMsg(goal.To<FLU>(), new QuaternionMsg()));

        ros.Publish(topicName, msg);
    }

    protected override void OnActivated(ActivateEventArgs args)
    {
        if (args.interactorObject is XRRayInteractor)
        {
            XRRayInteractor rayInteractor = (XRRayInteractor)args.interactorObject;
            Vector3 hitPosition = new Vector3();
            Vector3 hitNormal = new Vector3();
            rayInteractor.TryGetHitInfo(out hitPosition, out hitNormal, out _, out _);
            GoToPoint(transform.InverseTransformPoint(hitPosition));
        }
    }
}
