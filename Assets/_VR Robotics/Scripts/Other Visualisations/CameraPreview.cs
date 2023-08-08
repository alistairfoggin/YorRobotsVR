using UnityEngine;

public class CameraPreview : MonoBehaviour
{
    private CameraSubscriber m_Subscriber;

    // Start is called before the first frame update
    void Start()
    {
        m_Subscriber = CameraSubscriber.GetOrCreateInstance();
        GetComponent<MeshRenderer>().material.mainTexture = m_Subscriber.ImageTexture;
    }
}
