using RosMessageTypes.Nav;
using Unity.Collections;
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
    private float m_LastDrawingFrameTime;


    public delegate void TransformUpdateDelegate(Vector3 localPosition, Quaternion localRotation);
    public TransformUpdateDelegate transformUpdateDelegate;

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

    private void UpdateMap(OccupancyGridMsg occupancyGridMsg)
    {
        if (Time.time > m_LastDrawingFrameTime + 1)
            RecalculateMap(occupancyGridMsg);

        m_LastDrawingFrameTime = Time.time;
    }

    private void RecalculateMap(OccupancyGridMsg msg)
    {
        GenerateMesh(msg);

        Vector3 origin = msg.info.origin.position.From<FLU>();
        Quaternion rotation = msg.info.origin.orientation.From<FLU>();
        rotation.eulerAngles += new Vector3(0, -90, 0);
        float scale = msg.info.resolution;

        // offset the mesh by half a grid square, because the message's position defines the CENTER of grid square 0,0
        Vector3 drawOrigin = origin - rotation * new Vector3(scale * 0.5f, 0, scale * 0.5f);

        TFFrame tfFrame = m_TFSystem.GetTransform(msg.header);
        drawOrigin = tfFrame.TransformPoint(drawOrigin);
        rotation = Quaternion.Euler(rotation.eulerAngles + tfFrame.rotation.eulerAngles);

        transformUpdateDelegate.Invoke(drawOrigin, rotation);
    }

    private void GenerateMesh(OccupancyGridMsg msg)
    {
        int width = (int)msg.info.width;
        int height = (int)msg.info.height;
        float resolution = msg.info.resolution;

        int numVertices = (width + 1) * (height + 1);

        NativeArray<Vector3> vertexBuffer = new NativeArray<Vector3>(numVertices, Allocator.Persistent);
        NativeArray<Vector2> uvBuffer = new NativeArray<Vector2>(numVertices, Allocator.Persistent);
        NativeArray<int> triangleBuffer = new NativeArray<int>(width * height * 6, Allocator.Persistent);
        int i = 0;
        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                Vector3 vertex = new Vector3(x * resolution, 0, y * resolution);

                int threshold = 50;
                if (x < width && y < height && msg.data[x + width * y] > threshold // Check upper right of vertex
                    || (x > 0 && y < height && msg.data[x - 1 + width * y] > threshold) // Check upper left of vertex
                    || (x < width && y > 0 && msg.data[x + width * (y - 1)] > threshold) // Check lower right of vertex
                    || (x > 0 && y > 0 && msg.data[x - 1 + width * (y - 1)] > threshold)) // Check lower left of vertex
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
        mesh.Clear();
        mesh.SetVertices(vertexBuffer);
        mesh.SetUVs(0, uvBuffer);
        mesh.SetTriangles(triangleBuffer.ToArray(), 0);
        mesh.RecalculateNormals();

        vertexBuffer.Dispose();
        uvBuffer.Dispose();
        triangleBuffer.Dispose();
    }
}
