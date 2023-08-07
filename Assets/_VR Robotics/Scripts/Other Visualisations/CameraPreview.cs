using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class CameraPreview : MonoBehaviour
{
    ROSConnection m_ROSConnection;
    Texture2D m_Texture;

    // Start is called before the first frame update
    void Start()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_Texture = new Texture2D(2, 2);
        GetComponent<MeshRenderer>().material.mainTexture = m_Texture;

        m_ROSConnection.Subscribe<CompressedImageMsg>("/camera/image_raw/compressed", UpdateImage);
    }

    private void UpdateImage(CompressedImageMsg msg)
    {
        ImageConversion.LoadImage(m_Texture, msg.data);
    }
}
