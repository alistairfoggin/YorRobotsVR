using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using UnityEngine.InputSystem;

public class TeleoperationController : MonoBehaviour
{
    public bool PublishCmdVel { get; set; } = false;
    public string DirectMovementTopicName = "/cmd_vel";
    public string GoalPoseTopicName = "/goal_pose";
    [SerializeField]
    float movementSpeed = 0.2f;
    [SerializeField]
    float turningSpeed = 1.0f;
    [SerializeField]
    float messageDelay = 0.1f;

    float timePassed = 0f;
    Vector2 input = Vector2.zero;
    ROSConnection m_RosConnection;

    [SerializeField]
    InputActionReference movement;

    void Start()
    {
        m_RosConnection = ROSConnection.GetOrCreateInstance();
        m_RosConnection.RegisterPublisher<TwistMsg>(DirectMovementTopicName);
        m_RosConnection.RegisterPublisher<PoseStampedMsg>(GoalPoseTopicName);

        // Allow user input to directly move the robot as an alternative to the joysticks
        movement.action.performed += MovementPerformed;
    }

    void MovementPerformed(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }

    void Update()
    {
        timePassed += Time.deltaTime;
        // Check if it should send an update to the movement topic
        if (PublishCmdVel && timePassed > messageDelay)
        {
            // Calculate the movement and turning speeds given the input amounts
            TwistMsg msg = new TwistMsg(
                new Vector3Msg(input.y * movementSpeed, 0, 0),
                new Vector3Msg(0, 0, -input.x * turningSpeed));

            m_RosConnection.Publish(DirectMovementTopicName, msg);
            timePassed = 0;
        }
    }
    public void GoToPoint(Vector3 goal)
    {
        // Calculate the ROS destination goal and publish it
        PoseStampedMsg msg = new PoseStampedMsg(
            new HeaderMsg(new TimeMsg(), "map"),
            new PoseMsg(goal.To<FLU>(), new QuaternionMsg()));

        m_RosConnection.Publish(GoalPoseTopicName, msg);
    }

    public void UpdateForwardsMovement(float y)
    {
        input.y = y;
    }

    public void UpdateTurningMovement(float x)
    {
        input.x = x;
    }
}
