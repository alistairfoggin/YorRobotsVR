using UnityEngine;

public class LightProjection : MonoBehaviour
{
    RenderTexture m_RenderTexture;
    CameraSubscriber m_Subscriber;

    // Start is called before the first frame update
    void Start()
    {
        m_Subscriber = CameraSubscriber.GetOrCreateInstance();
        m_Subscriber.ImageUpdatedAction += ImageUpdated;
    }

    private void ImageUpdated()
    {
        // Create the render texture if it does not exist or if it is the wrong resolution
        if (m_RenderTexture == null || m_RenderTexture.width != m_Subscriber.ImageTexture.width || m_RenderTexture.height != m_Subscriber.ImageTexture.height)
        {
            if (m_RenderTexture != null)
                m_RenderTexture.DiscardContents();
            m_RenderTexture = new RenderTexture(m_Subscriber.ImageTexture.width, m_Subscriber.ImageTexture.height, 0);
            m_RenderTexture.autoGenerateMips = false;
            m_RenderTexture.Create();
        }

        // Every time an image is decoded, copy it over to the render texture. It needs to be a render texture because otherwise it fills up the light
        // cookie atlas, leading to a very low resolution projection
        Graphics.Blit(m_Subscriber.ImageTexture, m_RenderTexture);
        GetComponent<Light>().cookie = m_RenderTexture;
    }
}
