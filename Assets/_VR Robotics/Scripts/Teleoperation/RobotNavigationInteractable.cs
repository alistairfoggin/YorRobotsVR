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
    [SerializeField]
    private TeleoperationController m_TeleoperationController;

    private void Start()
    {
        m_TFSystem = TFSystem.GetOrCreateInstance();

        m_IndicatorOffset = m_DirectionIndicator.transform.position - transform.position;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        // When ray interactor selects this interactable, check the currently selected tool and call the appropriate action
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
        // Reset controls when deselected
        m_RayInteractor = null;
        m_DirectionIndicator.SetActive(false);
    }

    private void OnDestinationSelect()
    {
        // Get the hit location
        Vector3 hitPosition;
        Vector3 hitNormal;
        m_RayInteractor.TryGetHitInfo(out hitPosition, out hitNormal, out _, out _);

        // Ensure that the hit is on top of the interactable
        if (Vector3.Dot(hitNormal.normalized, transform.up) < 0.95f)
        {
            return;
        }

        Vector3 destination;
        // Get the local position but keep the scale as normal
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

        // Convert it to ROS local space relative to the map
        destination = m_TFSystem.GetTransform("map", 0).InverseTransformPoint(destination);
        destination.y = 0;
        
        // Send the destination to the robot
        m_TeleoperationController.GoToPoint(destination);
    }
    private void OnMoveSelectEntered()
    {
        // Get the location grabbed and find the fixed offset to keep
        RaycastHit hit;
        m_RayInteractor.TryGetCurrent3DRaycastHit(out hit);
        m_AttachOffet = hit.point - m_RobotWorldWrapperTransform.position;
    }

    private void OnRotateSelectEntered()
    {
        // Show the rotation indicator at the centre to show the pivot point
        m_DirectionIndicator.transform.position = transform.position + m_IndicatorOffset;
        m_DirectionIndicator.SetActive(true);

        // Get the vector and angle from the pivot to the grab point
        RaycastHit hit;
        m_RayInteractor.TryGetCurrent3DRaycastHit(out hit);
        m_AttachOffet = hit.point - transform.position;
        m_OffsetAngle = Vector3.SignedAngle(m_RobotWorldWrapperTransform.forward, m_AttachOffet.normalized, m_RobotWorldWrapperTransform.up);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        // Only run this in the dynamic phase which is called during Update()
        if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic) { return; }

        if (mapControlMode == MapControlMode.Move && m_RayInteractor != null)
        {
            // Get the updated grab position and move the map to keep the constant offset
            RaycastHit hit;
            if (m_RayInteractor.TryGetCurrent3DRaycastHit(out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Robot"))
            {
                m_RobotWorldWrapperTransform.position = hit.point - m_AttachOffet;
            }
        }
        else if (mapControlMode == MapControlMode.Rotate && m_RayInteractor != null)
        {
            // Get the updated grab position and rotate the map around the pivot to maintain the constant angle
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
