using RosMessageTypes.Sensor;
using System.Threading.Tasks;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.Events;

public class CameraSubscriber : MonoBehaviour
{
    public Texture2D ImageTexture { get; private set; }

    private Task<bool> m_DecodingTask;
    private AsyncImageLoader.LoaderSettings m_LoaderSettings;

    public UnityAction ImageUpdatedAction;

    private static CameraSubscriber cameraSubscriber;
    public static CameraSubscriber GetOrCreateInstance()
    {
        if (cameraSubscriber == null)
        {
            cameraSubscriber = FindObjectOfType<CameraSubscriber>();
        }
        return cameraSubscriber;
    }

    void Awake()
    {
        ImageTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        ImageTexture.wrapMode = TextureWrapMode.Clamp;
        ImageTexture.filterMode = FilterMode.Bilinear;

        m_LoaderSettings = new AsyncImageLoader.LoaderSettings
        {
            generateMipmap = false,
            mipmapCount = 1,
            autoMipmapCount = false,
            logException = true,
            format = AsyncImageLoader.FreeImage.Format.FIF_JPEG,
        };
        var rosConnection = ROSConnection.GetOrCreateInstance();
        rosConnection.Subscribe<CompressedImageMsg>("/camera/image_raw/compressed", UpdateImage);
    }

    private void UpdateImage(CompressedImageMsg msg)
    {
        if (m_DecodingTask == null)
        {
            m_DecodingTask = AsyncImageLoader.LoadImageAsync(ImageTexture, msg.data, m_LoaderSettings);
        }
        else if (m_DecodingTask.IsCompleted)
        {
            ImageUpdatedAction();
            m_DecodingTask = AsyncImageLoader.LoadImageAsync(ImageTexture, msg.data, m_LoaderSettings);
        }
    }
}
