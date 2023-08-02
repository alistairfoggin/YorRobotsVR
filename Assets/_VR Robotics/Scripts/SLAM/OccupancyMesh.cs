using RosMessageTypes.Nav;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class OccupancyMesh : MonoBehaviour
{
    OccupancyMeshGenerator m_MeshGenerator;

    // Start is called before the first frame update
    void Start()
    {
        m_MeshGenerator = OccupancyMeshGenerator.GetOrCreateInstance();
        GetComponent<MeshFilter>().mesh = m_MeshGenerator.mesh;
        m_MeshGenerator.transformUpdateDelegate += UpdateTransform;
    }

    void UpdateTransform(Vector3 localPosition, Quaternion localRotation)
    {
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
    }
}