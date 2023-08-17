using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class OccupancyMesh : MonoBehaviour
{
    OccupancyMeshGenerator m_MeshGenerator;
    [SerializeField]
    private Transform m_Bounds;

    void Start()
    {
        m_MeshGenerator = OccupancyMeshGenerator.GetOrCreateInstance();
        GetComponent<MeshFilter>().mesh = m_MeshGenerator.mesh;
        UpdateTransform(m_MeshGenerator.DrawOrigin, m_MeshGenerator.DrawRotation);
        m_MeshGenerator.transformUpdateDelegate += UpdateTransform;
    }

    void UpdateTransform(Vector3 localPosition, Quaternion localRotation)
    {
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;

        // If there are any bounds, update the dimensions.
        if (m_Bounds != null)
        {
            m_Bounds.localScale = new Vector3(m_MeshGenerator.TextureDimensions.x, m_MeshGenerator.TextureDimensions.y, 1f);
            // localPosition is from the upper left point, shift the bounds by half of the scale so make the origin of the mesh be the upper left corner
            m_Bounds.localPosition = new Vector3(localPosition.x - m_Bounds.localScale.y / 2f, -0.001f, localPosition.z + m_Bounds.localScale.x / 2f );
        }
    }
}
