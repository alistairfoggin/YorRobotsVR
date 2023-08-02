using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using UnityEngine.UI;
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

    [SerializeField]
    Sprite m_DestinationSprite;
    [SerializeField]
    Sprite m_MoveSprite;
    [SerializeField]
    Sprite m_RotateSprite;
    [SerializeField]
    Image m_ControllerImage;
    public void SetDestinationControl()
    {
        mapControlMode = MapControlMode.Destination;
        m_ControllerImage.sprite = m_DestinationSprite;
    }
    public void SetMoveControl()
    {
        mapControlMode = MapControlMode.Move;
        m_ControllerImage.sprite = m_MoveSprite;
    }
    public void SetRotateControl()
    {
        mapControlMode = MapControlMode.Rotate;
        m_ControllerImage.sprite = m_RotateSprite;
    }

    ROSConnection m_ROSConnection;
    ROSTime m_ROSTime;
    TFSystem m_TFSystem;
    public string TopicName = "/goal_pose";

    XRRayInteractor m_RayInteractor;
    Vector3 m_AttachOffet;
    float m_OffsetAngle;

    [SerializeField]
    Transform m_MapCentreTransform;
    [SerializeField]
    Transform m_RobotWorldWrapperTransform;
    [SerializeField]
    GameObject m_DirectionIndicator;
    private Vector3 m_IndicatorOffset;

    private void Start()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_ROSConnection.RegisterPublisher<PoseStampedMsg>(TopicName);

        m_ROSTime = ROSTime.GetOrCreateInstance();

        m_TFSystem = TFSystem.GetOrCreateInstance();

        m_IndicatorOffset = m_DirectionIndicator.transform.position - transform.position;
    }

    public void GoToPoint(Vector3 goal)
    {
        PoseStampedMsg msg = new PoseStampedMsg(
            new HeaderMsg(m_ROSTime.LatestTimeMsg, "map"),
            new PoseMsg(goal.To<FLU>(), new QuaternionMsg()));

        m_ROSConnection.Publish(TopicName, msg);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRRayInteractor)
        {
            m_RayInteractor = (XRRayInteractor)args.interactorObject;
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
        m_RayInteractor = null;
        m_DirectionIndicator.SetActive(false);
    }

    private void OnDestinationSelect()
    {
        Vector3 hitPosition;
        Vector3 hitNormal;
        m_RayInteractor.TryGetHitInfo(out hitPosition, out hitNormal, out _, out _);

        if (Vector3.Dot(hitNormal.normalized, transform.up) < 0.95f)
        {
            return;
        }

        Vector3 destination;
        if (m_MapCentreTransform != null)
        {
            destination = m_MapCentreTransform.InverseTransformPoint(hitPosition);
            destination.Scale(m_MapCentreTransform.GetComponentInParent<Transform>().localScale);
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
        m_RayInteractor.TryGetCurrent3DRaycastHit(out hit);
        m_AttachOffet = hit.point - m_RobotWorldWrapperTransform.position;
    }

    private void OnRotateSelectEntered()
    {
        m_DirectionIndicator.transform.position = transform.position + m_IndicatorOffset;
        m_DirectionIndicator.SetActive(true);
        RaycastHit hit;
        m_RayInteractor.TryGetCurrent3DRaycastHit(out hit);
        m_AttachOffet = hit.point - transform.position;
        m_OffsetAngle = Vector3.SignedAngle(m_RobotWorldWrapperTransform.forward, m_AttachOffet.normalized, m_RobotWorldWrapperTransform.up);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic) { return; }

        if (mapControlMode == MapControlMode.Move && m_RayInteractor != null)
        {
            RaycastHit hit;
            if (m_RayInteractor.TryGetCurrent3DRaycastHit(out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Robot"))
            {
                m_RobotWorldWrapperTransform.position = hit.point - m_AttachOffet;
            }
        }
        else if (mapControlMode == MapControlMode.Rotate && m_RayInteractor != null)
        {
            RaycastHit hit;
            if (m_RayInteractor.TryGetCurrent3DRaycastHit(out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Robot"))
            {
                Vector3 newOffset = hit.point - transform.position;
                float newOffsetAngle = Vector3.SignedAngle(m_RobotWorldWrapperTransform.forward, newOffset.normalized, m_RobotWorldWrapperTransform.up);
                float deltaRotation = newOffsetAngle - m_OffsetAngle;
                m_RobotWorldWrapperTransform.RotateAround(transform.position, transform.up, deltaRotation);
            }
        }
    }
}
