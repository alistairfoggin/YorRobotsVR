using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class OccupancyMesh : MonoBehaviour
{
    OccupancyMeshGenerator m_MeshGenerator;
    private BoxCollider m_BoxCollider;

    // Start is called before the first frame update
    void Start()
    {
        m_MeshGenerator = OccupancyMeshGenerator.GetOrCreateInstance();
        GetComponent<MeshFilter>().mesh = m_MeshGenerator.mesh;
        m_BoxCollider = GetComponent<BoxCollider>();
        m_MeshGenerator.transformUpdateDelegate += UpdateTransform;
    }

    void UpdateTransform(Vector3 localPosition, Quaternion localRotation)
    {
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;

        if (m_BoxCollider != null)
        {
            m_BoxCollider.size = new Vector3(m_MeshGenerator.TextureDimensions.x, .1f, m_MeshGenerator.TextureDimensions.y);
            m_BoxCollider.center = m_BoxCollider.size / 2f;
        }
    }
}
