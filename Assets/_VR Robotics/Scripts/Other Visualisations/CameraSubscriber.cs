using RosMessageTypes.Sensor;
using System.Threading.Tasks;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class CameraSubscriber : MonoBehaviour
{
    public Texture2D ImageTexture { get; private set; }

    private Task<Texture2D> decodingTask;

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
        var rosConnection = ROSConnection.GetOrCreateInstance();
        rosConnection.Subscribe<CompressedImageMsg>("/camera/image_raw/compressed", UpdateImage);
    }

    private void UpdateImage(CompressedImageMsg msg)
    {
        if (decodingTask == null)
        {
            decodingTask = AsyncImageLoader.CreateFromImageAsync(msg.data);
        }
        else if (decodingTask.IsCompleted)
        {
            ImageTexture = decodingTask.Result;
            decodingTask = AsyncImageLoader.CreateFromImageAsync(msg.data);
        }
    }
}
