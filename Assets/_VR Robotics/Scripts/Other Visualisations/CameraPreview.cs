using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class CameraPreview : MonoBehaviour
{
    ROSConnection m_ROSConnection;
    Material m_Material;
    Texture2D m_Texture;

    // Start is called before the first frame update
    void Start()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_Material = GetComponent<MeshRenderer>().material;

        m_ROSConnection.Subscribe<ImageMsg>("/camera/image_raw", UpdateImage);
    }

    private void UpdateImage(ImageMsg msg)
    {
        if (m_Texture == null)
        {
            m_Texture = new Texture2D((int)msg.width, (int)msg.height, TextureFormat.RGB24, true);
            m_Texture.wrapMode = TextureWrapMode.Clamp;
            m_Texture.filterMode = FilterMode.Trilinear;
            m_Material.mainTexture = m_Texture;
        }
        else if (msg.width != m_Texture.width || msg.height != m_Texture.height)
        {
            m_Texture.Reinitialize((int)msg.width, (int)msg.height);
        }

        m_Texture.SetPixelData(msg.data, 0);
        m_Texture.Apply();
    }
}
