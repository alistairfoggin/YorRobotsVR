using RosMessageTypes.Nav;
using Unity.Collections;
using Unity.Jobs;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class OccupancyMesh : MonoBehaviour
{
    OccupancyMeshGenerator m_MeshGenerator;

    // Start is called before the first frame update
    void Start()
    {
        m_MeshGenerator = OccupancyMeshGenerator.GetOrCreateInstance();
        GetComponent<MeshFilter>().mesh = m_MeshGenerator.mesh;
        m_MeshGenerator.transformUpdateDelegate += UpdateTransform;
    }

    void UpdateTransform(Vector3 localPosition, Quaternion localRotation)
    {
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
    }
}

class OccupancyMeshGenerator
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
    private NativeArray<int> triangleBuffer;

    public Mesh mesh { get; private set; }
    public static OccupancyMeshGenerator GetOrCreateInstance()
    {
        if (meshGenerator != null)
        {
            return meshGenerator;
        }
        meshGenerator = new OccupancyMeshGenerator();
        return meshGenerator;
    }

    private OccupancyMeshGenerator()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_ROSConnection.Subscribe<OccupancyGridMsg>("/map", UpdateMap);

        m_TFSystem = TFSystem.GetOrCreateInstance();

        mesh = new Mesh();
    }


    private void UpdateMap(OccupancyGridMsg msg)
    {
        if (handle != null)
        {
            if (!handle.Value.IsCompleted) return;
            handle.Value.Complete();
            mesh.Clear();
            mesh.SetVertices(vertexBuffer);
            mesh.SetUVs(0, uvBuffer);
            mesh.SetTriangles(triangleBuffer.ToArray(), 0);
            mesh.RecalculateNormals();

            vertexBuffer.Dispose();
            uvBuffer.Dispose();
            triangleBuffer.Dispose();
            data.Dispose();

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
        }

        StartGeneratingMesh(msg);
        m_LastMsg = msg;
    }

    private void StartGeneratingMesh(OccupancyGridMsg msg)
    {
        int width = (int)msg.info.width;
        int height = (int)msg.info.height;
        float resolution = msg.info.resolution;

        int numVertices = (width + 1) * (height + 1);

        vertexBuffer = new NativeArray<Vector3>(numVertices, Allocator.Persistent);
        uvBuffer = new NativeArray<Vector2>(numVertices, Allocator.Persistent);
        triangleBuffer = new NativeArray<int>(width * height * 6, Allocator.Persistent);
        data = new NativeArray<sbyte>(msg.data, Allocator.Persistent);

        MeshGenerationJob job = new MeshGenerationJob
        {
            width = width,
            height = height,
            resolution = resolution,
            data = data,
            vertexBuffer = vertexBuffer,
            uvBuffer = uvBuffer,
            triangleBuffer = triangleBuffer
        };
        handle = job.Schedule();
    }

    struct MeshGenerationJob : IJob
    {
        public int height;
        public int width;
        public float resolution;

        [ReadOnly]
        public NativeArray<sbyte> data;

        public NativeArray<Vector3> vertexBuffer;
        public NativeArray<Vector2> uvBuffer;
        public NativeArray<int> triangleBuffer;

        public void Execute()
        {
            int i = 0;
            for (int y = 0; y <= height; y++)
            {
                for (int x = 0; x <= width; x++)
                {
                    Vector3 vertex = new Vector3(x * resolution, 0, y * resolution);

                    int threshold = 50;
                    if (x < width && y < height && data[x + width * y] > threshold // Check upper right of vertex
                        || (x > 0 && y < height && data[x - 1 + width * y] > threshold) // Check upper left of vertex
                        || (x < width && y > 0 && data[x + width * (y - 1)] > threshold) // Check lower right of vertex
                        || (x > 0 && y > 0 && data[x - 1 + width * (y - 1)] > threshold)) // Check lower left of vertex
                    {
                        vertex.y = 0.4f;
                    }

                    vertexBuffer[x + y * (width + 1)] = vertex;
                    uvBuffer[x + y * (width + 1)] = new Vector2(x / width, 1 - y / height);

                    if (y < height && x < width)
                    {
                        triangleBuffer[6 * i] = x + y * (width + 1);
                        triangleBuffer[6 * i + 1] = x + (y + 1) * (width + 1);
                        triangleBuffer[6 * i + 2] = x + 1 + y * (width + 1);

                        triangleBuffer[6 * i + 3] = x + (y + 1) * (width + 1);
                        triangleBuffer[6 * i + 4] = x + 1 + (y + 1) * (width + 1);
                        triangleBuffer[6 * i + 5] = x + 1 + y * (width + 1);
                        i++;
                    }
                }
            }
        }
    }
}
