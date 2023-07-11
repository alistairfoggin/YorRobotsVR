using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;

[RequireComponent(typeof(ParticleSystem))]
public class LidarParticleSystem : MonoBehaviour
{
    private ROSConnection m_RosConnection;
    private TFSystem m_TFSystem;
    private ParticleSystem m_ParticleSystem;
    private LaserScanMsg m_LaserScan;

    // Start is called before the first frame update
    void Start()
    {
        m_RosConnection = ROSConnection.GetOrCreateInstance();
        m_RosConnection.Subscribe<LaserScanMsg>("/scan", LaserScanChange);

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

        while (currentAngleRad <= m_LaserScan.angle_max)
        {
            if (m_LaserScan.ranges[i] <= m_LaserScan.range_max && m_LaserScan.ranges[i] >= m_LaserScan.range_min)
            {
                Quaternion rotation = Quaternion.Euler(0, -Mathf.Rad2Deg * currentAngleRad, 0);
                Vector3 localPosition = rotation * Vector3.forward * m_LaserScan.ranges[i];

                TFFrame tfFrame = m_TFSystem.GetTransform(m_LaserScan.header);
                localPosition = tfFrame.TransformPoint(localPosition);

                var emitParams = new ParticleSystem.EmitParams();
                emitParams.position = transform.TransformPoint(localPosition);
                m_ParticleSystem.Emit(emitParams, 1);
            }

            i++;
            currentAngleRad += m_LaserScan.angle_increment;
        }
    }
}
