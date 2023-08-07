using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightProjection : MonoBehaviour
{
    Light m_Light;
    CameraSubscriber m_Subscriber;
    // Start is called before the first frame update
    void Start()
    {
        m_Light = GetComponent<Light>();
        m_Subscriber = CameraSubscriber.GetOrCreateInstance();
    }

    private void Update()
    {
        m_Light.cookie = m_Subscriber.ImageTexture;
    }
}
