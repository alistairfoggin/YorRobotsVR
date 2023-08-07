using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class CameraPreview : MonoBehaviour
{
    private Material m_Material;
    private CameraSubscriber m_Subscriber;

    // Start is called before the first frame update
    void Start()
    {
        m_Material = GetComponent<MeshRenderer>().material;
        m_Subscriber = CameraSubscriber.GetOrCreateInstance();
    }
    private void Update()
    {
        m_Material.mainTexture = m_Subscriber.ImageTexture;
    }
}
