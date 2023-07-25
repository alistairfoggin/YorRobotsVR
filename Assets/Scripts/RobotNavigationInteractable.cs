using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using System;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RobotNavigationInteractable : XRBaseInteractable
{
    public enum MapControlMode
    {
        Destination,
        Move,
        Rotate,
    }

    MapControlMode mapControlMode = MapControlMode.Destination;
    public void SetDestinationControl() { mapControlMode = MapControlMode.Destination; }
    public void SetMoveControl() { mapControlMode = MapControlMode.Move; }
    public void SetRotateControl() { mapControlMode = MapControlMode.Rotate; }

    ROSConnection m_ROSConnection;
    ROSTime m_ROSTime;
    TFSystem m_TFSystem;
    public string topicName = "/goal_pose";

    XRRayInteractor rayInteractor;
    Vector3 attachOffet;
    float offsetAngle;

    [SerializeField]
    Transform mapCentreTransform;
    [SerializeField]
    Transform robotWorldWrapperTransform;

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
            rayInteractor = (XRRayInteractor)args.interactorObject;
            switch (mapControlMode)
            {
                case MapControlMode.Destination:
                    OnDestinationSelect();
                    break;
                case MapControlMode.Move:
                    OnMoveSelectEntered();
                    break;
                case MapControlMode.Rotate:
                    OnRotateSelectEntered();
                    break;
            }
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        rayInteractor = null;
    }

    private void OnDestinationSelect()
    {
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
    private void OnMoveSelectEntered()
    {
        RaycastHit hit;
        rayInteractor.TryGetCurrent3DRaycastHit(out hit);
        attachOffet = hit.point - robotWorldWrapperTransform.position;
    }

    private void OnRotateSelectEntered()
    {
        RaycastHit hit;
        rayInteractor.TryGetCurrent3DRaycastHit(out hit);
        attachOffet = hit.point - robotWorldWrapperTransform.position;
        offsetAngle = Vector3.SignedAngle(robotWorldWrapperTransform.forward, attachOffet.normalized, robotWorldWrapperTransform.up);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic) { return; }

        if (mapControlMode == MapControlMode.Move && rayInteractor != null)
        {
            RaycastHit hit;
            if (rayInteractor.TryGetCurrent3DRaycastHit(out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Robot"))
            {
                robotWorldWrapperTransform.position = hit.point - attachOffet;
            }
        }
        else if (mapControlMode == MapControlMode.Rotate && rayInteractor != null)
        {
            RaycastHit hit;
            if (rayInteractor.TryGetCurrent3DRaycastHit(out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Robot"))
            {
                Vector3 newOffset = hit.point - robotWorldWrapperTransform.position;
                float newOffsetAngle = Vector3.SignedAngle(robotWorldWrapperTransform.forward, newOffset.normalized, robotWorldWrapperTransform.up);
                float deltaRotation = newOffsetAngle - offsetAngle;
                robotWorldWrapperTransform.RotateAround(transform.position, transform.up, deltaRotation);
            }
        }
    }
}
