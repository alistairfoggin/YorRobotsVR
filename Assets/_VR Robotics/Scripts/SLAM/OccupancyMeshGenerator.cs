using RosMessageTypes.Nav;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class OccupancyMeshGenerator : MonoBehaviour
{
    private static OccupancyMeshGenerator meshGenerator;

    private ROSConnection m_ROSConnection;
    private TFSystem m_TFSystem;
    private OccupancyGridMsg m_LastMsg;

    public delegate void TransformUpdateDelegate(Vector3 localPosition, Quaternion localRotation);
    public TransformUpdateDelegate transformUpdateDelegate;
    private JobHandle? handle = null;
    private NativeArray<sbyte> data;
    private NativeArray<Vector3> vertexBuffer;
    private NativeArray<Vector2> uvBuffer;
    private NativeList<int> triangleBuffer;
    private NativeArray<Color32> colorBuffer;
    private int width;
    private int height;

    public Mesh mesh { get; private set; }
    public Texture2D OccupancyTexture { get; private set; }

    public static OccupancyMeshGenerator GetOrCreateInstance()
    {
        if (meshGenerator != null)
        {
            return meshGenerator;
        }
        meshGenerator = FindObjectOfType<OccupancyMeshGenerator>();
        return meshGenerator;
    }


    void Awake()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_ROSConnection.Subscribe<OccupancyGridMsg>("/map", UpdateMap);

        m_TFSystem = TFSystem.GetOrCreateInstance();

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // This has limited support on lower end platforms

        OccupancyTexture = new Texture2D(10, 10, TextureFormat.RGB24, false);
        OccupancyTexture.filterMode = FilterMode.Point;
    }

    void OnDestroy()
    {
        m_ROSConnection.Unsubscribe("/map");
        if (handle != null)
        {
            handle.Value.Complete();
            data.Dispose();
            vertexBuffer.Dispose();
            uvBuffer.Dispose();
            triangleBuffer.Dispose();
            colorBuffer.Dispose();
        }
    }

    private void LateUpdate()
    {
        if (handle == null || !handle.Value.IsCompleted) return;
        handle.Value.Complete();

        // Update mesh
        mesh.Clear();
        mesh.SetVertices(vertexBuffer);
        mesh.SetUVs(0, uvBuffer);
        mesh.SetTriangles(triangleBuffer.ToArray(), 0);
        mesh.RecalculateNormals();

        if (OccupancyTexture.width != width || OccupancyTexture.height != height)
        {
            OccupancyTexture.Reinitialize(width, height);
        }
        OccupancyTexture.SetPixels32(colorBuffer.ToArray());
        OccupancyTexture.Apply();

        // Dispose of non-GC data
        vertexBuffer.Dispose();
        uvBuffer.Dispose();
        triangleBuffer.Dispose();
        colorBuffer.Dispose();
        data.Dispose();

        // Positioning of object
        Vector3 origin = m_LastMsg.info.origin.position.From<FLU>();
        Quaternion rotation = m_LastMsg.info.origin.orientation.From<FLU>();
        rotation.eulerAngles += new Vector3(0, -90, 0);
        float scale = m_LastMsg.info.resolution;

        // offset the mesh by half a grid square, because the message's position defines the CENTER of grid square 0,0
        Vector3 drawOrigin = origin - rotation * new Vector3(scale * 0.5f, 0, scale * 0.5f);

        TFFrame tfFrame = m_TFSystem.GetTransform(m_LastMsg.header);
        drawOrigin = tfFrame.TransformPoint(drawOrigin);
        rotation = Quaternion.Euler(rotation.eulerAngles + tfFrame.rotation.eulerAngles);

        transformUpdateDelegate.Invoke(drawOrigin, rotation);
        handle = null;
    }

    private void UpdateMap(OccupancyGridMsg msg)
    {
        // Only update if previous map has finished generating
        if (handle != null) return;
        StartGeneratingMesh(msg);
        m_LastMsg = msg;
    }

    private void StartGeneratingMesh(OccupancyGridMsg msg)
    {
        width = (int)msg.info.width;
        height = (int)msg.info.height;
        float resolution = msg.info.resolution;

        int numVertices = (width + 1) * (height + 1) * 2;

        vertexBuffer = new NativeArray<Vector3>(numVertices, Allocator.Persistent);
        uvBuffer = new NativeArray<Vector2>(numVertices, Allocator.Persistent);
        triangleBuffer = new NativeList<int>(width * height * 12, Allocator.Persistent);
        colorBuffer = new NativeArray<Color32>((int)(msg.info.width * msg.info.height), Allocator.Persistent);
        data = new NativeArray<sbyte>(msg.data, Allocator.Persistent);

        MeshGenerationJob job = new MeshGenerationJob
        {
            width = width,
            height = height,
            resolution = resolution,
            data = data,
            vertexBuffer = vertexBuffer,
            uvBuffer = uvBuffer,
            triangleBuffer = triangleBuffer,
            colorBuffer = colorBuffer
        };
        handle = job.Schedule();
    }

    [BurstCompile(CompileSynchronously = true)]
    struct MeshGenerationJob : IJob
    {
        public int height;
        public int width;
        public float resolution;

        [ReadOnly]
        public NativeArray<sbyte> data;

        public NativeArray<Vector3> vertexBuffer;
        public NativeArray<Vector2> uvBuffer;
        public NativeList<int> triangleBuffer;
        public NativeArray<Color32> colorBuffer;


        private int numVerticesInLayer;
        private int threshold;

        public void Execute()
        {
            numVerticesInLayer = (width + 1) * (height + 1);
            triangleBuffer.Clear();
            threshold = 50;

            // Layer 1
            for (int y = 0; y <= height; y++)
            {
                for (int x = 0; x <= width; x++)
                {
                    Vector3 vertex = new Vector3(x * resolution, 0, y * resolution);

                    vertexBuffer[x + y * (width + 1)] = vertex;
                    uvBuffer[x + y * (width + 1)] = new Vector2((float)x / width, (float)y / height);

                    if (y < height && x < width && data[x + width * y] <= threshold)
                    {
                        triangleBuffer.Add(x + y * (width + 1));
                        triangleBuffer.Add(x + (y + 1) * (width + 1));
                        triangleBuffer.Add(x + 1 + y * (width + 1));

                        triangleBuffer.Add(x + (y + 1) * (width + 1));
                        triangleBuffer.Add(x + 1 + (y + 1) * (width + 1));
                        triangleBuffer.Add(x + 1 + y * (width + 1));

                        if (data[x + y * width] < 0)
                        {
                            colorBuffer[x + y * (width)] = Color.cyan;
                        }
                        else
                        {
                            float val = Mathf.Lerp(1f, 0f, (float)data[x + y * width] / 100);
                            colorBuffer[x + y * (width)] = new Color(val, val, val);
                        }
                    }
                }
            }

            // Layer 2
            for (int y = 0; y <= height; y++)
            {
                for (int x = 0; x <= width; x++)
                {
                    Vector3 vertex = new Vector3(x * resolution, 0.4f, y * resolution);

                    int index = x + y * (width + 1) + numVerticesInLayer;
                    vertexBuffer[index] = vertex;
                    uvBuffer[index] = new Vector2((float)x / width, (float)y / height);

                    if (y < height && x < width && data[x + width * y] > threshold)
                    {
                        triangleBuffer.Add(x + y * (width + 1) + numVerticesInLayer);
                        triangleBuffer.Add(x + (y + 1) * (width + 1) + numVerticesInLayer);
                        triangleBuffer.Add(x + 1 + y * (width + 1) + numVerticesInLayer);

                        triangleBuffer.Add(x + (y + 1) * (width + 1) + numVerticesInLayer);
                        triangleBuffer.Add(x + 1 + (y + 1) * (width + 1) + numVerticesInLayer);
                        triangleBuffer.Add(x + 1 + y * (width + 1) + numVerticesInLayer);

                        AddVerticalWalls(x, y);
                    }
                }
            }
        }

        // Only call for values above threshold
        private void AddVerticalWalls(int x, int y)
        {
            // Check left of point
            if (x > 0 && data[x - 1 + y * width] <= threshold)
            {
                triangleBuffer.Add(x + y * (width + 1));
                triangleBuffer.Add(x + (y + 1) * (width + 1) + numVerticesInLayer);
                triangleBuffer.Add(x + y * (width + 1) + numVerticesInLayer);

                triangleBuffer.Add(x + (y + 1) * (width + 1) + numVerticesInLayer);
                triangleBuffer.Add(x + y * (width + 1));
                triangleBuffer.Add(x + (y + 1) * (width + 1));
            }

            // Check below point
            if (y > 0 && data[x + (y - 1) * width] <= threshold)
            {
                triangleBuffer.Add(x + y * (width + 1));
                triangleBuffer.Add(x + y * (width + 1) + numVerticesInLayer);
                triangleBuffer.Add(x + 1 + y * (width + 1) + numVerticesInLayer);

                triangleBuffer.Add(x + y * (width + 1));
                triangleBuffer.Add(x + 1 + y * (width + 1) + numVerticesInLayer);
                triangleBuffer.Add(x + 1 + y * (width + 1));
            }

            // Check right of point
            if (x < width - 1 && data[x + 1 + y * width] <= threshold)
            {
                triangleBuffer.Add(x + 1 + y * (width + 1));
                triangleBuffer.Add(x + 1 + y * (width + 1) + numVerticesInLayer);
                triangleBuffer.Add(x + 1 + (y + 1) * (width + 1) + numVerticesInLayer);

                triangleBuffer.Add(x + 1 + (y + 1) * (width + 1) + numVerticesInLayer);
                triangleBuffer.Add(x + 1 + (y + 1) * (width + 1));
                triangleBuffer.Add(x + 1 + y * (width + 1));
            }

            // Check above point
            if (y < height - 1 && data[x + (y + 1) * width] <= threshold)
            {
                triangleBuffer.Add(x + (y + 1) * (width + 1));
                triangleBuffer.Add(x + 1 + (y + 1) * (width + 1) + numVerticesInLayer);
                triangleBuffer.Add(x + (y + 1) * (width + 1) + numVerticesInLayer);

                triangleBuffer.Add(x + (y + 1) * (width + 1));
                triangleBuffer.Add(x + 1 + (y + 1) * (width + 1));
                triangleBuffer.Add(x + 1 + (y + 1) * (width + 1) + numVerticesInLayer);
            }
        }
    }
}
