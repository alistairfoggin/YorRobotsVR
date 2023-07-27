using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class CameraDecal : MonoBehaviour
{
    ROSConnection m_ROSConnection;
    Material m_Material;
    Texture2D m_Texture;

    // Start is called before the first frame update
    void Start()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_Material = new Material(Shader.Find("Shader Graphs/ProjectorImage"));
        GetComponent<DecalProjector>().material = m_Material;

        m_ROSConnection.Subscribe<ImageMsg>("/camera/image_raw", UpdateImage);
        m_ROSConnection.Subscribe<CameraInfoMsg>("/camera/camera_info", UpdateProjection);
    }

    private void UpdateProjection(CameraInfoMsg msg)
    {
        Matrix4x4 msgMatrix = Matrix4x4.Perspective((float)msg.K[0], 1, 0.1f, 100);
        m_Material.SetMatrix("_Matrix4x4", msgMatrix.inverse);
    }

    private void UpdateImage(ImageMsg msg)
    {
        if (m_Texture == null)
        {
            m_Texture = new Texture2D((int)msg.width, (int)msg.height, TextureFormat.RGB24, true);
            m_Texture.wrapMode = TextureWrapMode.Clamp;
            m_Texture.filterMode = FilterMode.Trilinear;
            m_Material.SetTexture("_Image", m_Texture);
        }
        else if (msg.width != m_Texture.width || msg.height != m_Texture.height)
        {
            m_Texture.Reinitialize((int)msg.width, (int)msg.height);
        }

        m_Texture.SetPixelData(msg.data, 0);
        m_Texture.Apply();
    }
}
