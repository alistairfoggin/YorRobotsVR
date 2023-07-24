using RosMessageTypes.Sensor;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class LightProjection : MonoBehaviour
{
    ROSConnection m_ROSConnection;
    Light m_Light;
    Texture2D m_Texture;
    ImageMsg m_LastMsg;
    NativeArray<byte> pixels;
    JobHandle? handle = null;

    // Start is called before the first frame update
    void Start()
    {
        m_Light = GetComponent<Light>();

        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_ROSConnection.Subscribe<ImageMsg>("/camera/image_raw", UpdateImage);
    }

    private void UpdateImage(ImageMsg msg)
    {
        if (handle != null)
        {
            if (!handle.Value.IsCompleted) return;
            handle.Value.Complete();
            if (m_Texture == null)
            {
                m_Texture = new Texture2D((int)m_LastMsg.width, (int)m_LastMsg.height, TextureFormat.RGB24, true);
                m_Texture.wrapMode = TextureWrapMode.Clamp;
                m_Texture.filterMode = FilterMode.Trilinear;
                m_Light.cookie = m_Texture;
            }
            else if (m_LastMsg.width != m_Texture.width || m_LastMsg.height != m_Texture.height)
            {
                m_Texture.Reinitialize((int)m_LastMsg.width, (int)m_LastMsg.height);
            }
            m_Texture.SetPixelData(pixels, 0);
            m_Texture.Apply();
            pixels.Dispose();
        }

        pixels = new NativeArray<byte>(msg.data, Allocator.Persistent);
        FlipJob job = new FlipJob
        {
            height = (int)msg.height,
            step = (int)msg.step,
            pixels = pixels
        };
        handle = job.Schedule();
        m_LastMsg = msg;
    }

    [BurstCompile(CompileSynchronously = true)]
    struct FlipJob : IJob
    {
        public int height;
        public int step;
        public NativeArray<byte> pixels;

        public void Execute()
        {
            for (int y = 0; y < height / 2; y++)
            {
                int flippedYOffset = y * step;
                int dataYOffset = (height - y - 1) * step;
                for (int x = 0; x < step; x++)
                {
                    var tmp = pixels[x + flippedYOffset];
                    pixels[x + flippedYOffset] = pixels[x + dataYOffset];
                    pixels[x + dataYOffset] = tmp;
                }
            }
        }
    }
}
