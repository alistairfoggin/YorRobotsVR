using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class LidarParticleSystem : MonoBehaviour
{
    [SerializeField]
    private string m_LidarTopic = "/scan";

    private ROSConnection m_RosConnection;
    private TFSystem m_TFSystem;
    private ParticleSystem m_ParticleSystem;
    private LaserScanMsg m_LaserScan;

    // Start is called before the first frame update
    void Start()
    {
        m_RosConnection = ROSConnection.GetOrCreateInstance();
        m_RosConnection.Subscribe<LaserScanMsg>(m_LidarTopic, LaserScanChange);

        m_TFSystem = TFSystem.GetOrCreateInstance();

        m_ParticleSystem = GetComponent<ParticleSystem>();
    }

    void LaserScanChange(LaserScanMsg msg)
    {
        m_LaserScan = msg;
        SpawnLaserParticles();
    }

    void SpawnLaserParticles()
    {
        float currentAngleRad = m_LaserScan.angle_min;
        int i = 0;
        m_ParticleSystem.Clear();

        // rotate around LiDAR sensor and spawn particles at every point that has a valid distance
        while (currentAngleRad <= m_LaserScan.angle_max)
        {
            // Check if it is a valid distance
            if (m_LaserScan.ranges[i] <= m_LaserScan.range_max && m_LaserScan.ranges[i] >= m_LaserScan.range_min)
            {
                // Calculate angle and position of where the laser hit
                Quaternion rotation = Quaternion.Euler(0, -Mathf.Rad2Deg * currentAngleRad, 0);
                Vector3 localPosition = rotation * Vector3.forward * m_LaserScan.ranges[i];

                // transform the position into the robot coordinate frame
                TFFrame tfFrame = m_TFSystem.GetTransform(m_LaserScan.header);
                localPosition = tfFrame.TransformPoint(localPosition);

                // transform the position into unity world space and emit it
                var emitParams = new ParticleSystem.EmitParams();
                emitParams.position = transform.TransformPoint(localPosition);
                m_ParticleSystem.Emit(emitParams, 1);
            }

            i++;
            currentAngleRad += m_LaserScan.angle_increment;
        }
    }
}
