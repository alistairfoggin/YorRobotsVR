using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.InputSystem;

public class CmdVelController : MonoBehaviour
{
    ROSConnection m_RosConnection;
    public string topicName = "/cmd_vel";
    [SerializeField]
    float movementSpeed = 0.2f;
    [SerializeField]
    float turningSpeed = 1.0f;
    [SerializeField]
    float messageDelay = 0.1f;
    float timePassed = 0f;
    Vector2 input = Vector2.zero;

    [SerializeField]
    InputActionReference movement;

    // Start is called before the first frame update
    void Start()
    {
        m_RosConnection = ROSConnection.GetOrCreateInstance();
        m_RosConnection.RegisterPublisher<TwistMsg>(topicName);

        movement.action.performed += MovementPerformed;
    }

    void MovementPerformed(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;
        if (timePassed > messageDelay)
        {
            TwistMsg msg = new TwistMsg(
                new Vector3Msg(input.y * movementSpeed, 0, 0),
                new Vector3Msg(0, 0, -input.x * turningSpeed));

            m_RosConnection.Publish(topicName, msg);
            timePassed = 0;
        }
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
