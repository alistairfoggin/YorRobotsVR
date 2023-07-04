using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

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

    // Start is called before the first frame update
    void Start()
    {
        m_RosConnection = ROSConnection.GetOrCreateInstance();
        m_RosConnection.RegisterPublisher<TwistMsg>(topicName);
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;
        if (timePassed > messageDelay)
        {
            float turningInput = Input.GetAxis("Horizontal");
            float forwardInput = Input.GetAxis("Vertical");

            TwistMsg msg = new TwistMsg(
                new Vector3Msg(forwardInput * movementSpeed, 0, 0),
                new Vector3Msg(0, 0, -turningInput * turningSpeed));

            m_RosConnection.Publish(topicName, msg);
            timePassed = 0;
        }
    }
}
