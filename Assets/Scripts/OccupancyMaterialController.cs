using UnityEngine;

public class OccupancyMaterialController : MonoBehaviour
{
    private Material m_Material;

    [SerializeField]
    float radius = 4f;
    [SerializeField]
    bool useBounds = true;
    [SerializeField]
    Transform centreTransform;

    // Start is called before the first frame update
    void Start()
    {
        //m_ROSConnection = ROSConnection.GetOrCreateInstance();
        //m_ROSConnection.Subscribe<OdometryMsg>("/odom", UpdatePosition);

        m_Material = new Material(Shader.Find("Shader Graphs/Bounds"));
        m_Material.SetFloat("_Radius", radius);
        m_Material.SetInt("_Use_Bounds", useBounds ? 1 : 0);
        GetComponent<MeshRenderer>().material = m_Material;

        Vector3 scale = centreTransform.localScale;
        scale.x = 2 * radius;
        scale.z = 2 * radius;
        centreTransform.localScale = scale;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Material.GetFloat("_Radius") != radius)
        {
            m_Material.SetFloat("_Radius", radius);
            Vector3 scale = centreTransform.localScale;
            scale.x = 2 * radius;
            scale.z = 2 * radius;
            centreTransform.localScale = scale;
        }

        if ((m_Material.GetInt("_Use_Bounds") == 1) != useBounds)
        {
            m_Material.SetInt("_Use_Bounds", useBounds ? 1 : 0);
        }

        m_Material.SetVector("_Centre", new Vector2(centreTransform.position.x, centreTransform.position.z));
    }

    //void UpdatePosition(OdometryMsg msg)
    //{
    //    Vector3 position = msg.pose.pose.position.From<FLU>();
    //    position = Quaternion.Euler(0, 90, 0) * (position - transform.localPosition);

    //    TFFrame tfFrame = m_TFSystem.GetTransform(msg.header);
    //    position = tfFrame.TransformPoint(position);
    //    m_Material.SetVector("_Centre", new Vector2(position.x, position.z));
    //}
}
