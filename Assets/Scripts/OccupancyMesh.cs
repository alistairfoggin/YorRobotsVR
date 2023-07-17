using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using RosMessageTypes.Std;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class OccupancyMesh : MonoBehaviour
{
    MeshGenerator m_MeshGenerator;

    private OccupancyGridMsg m_Message;
    private float m_LastDrawingFrameTime;

    // Start is called before the first frame update
    void Start()
    {
        m_MeshGenerator = MeshGenerator.GetOrCreateInstance();
        GetComponent<MeshFilter>().mesh = m_MeshGenerator.mesh;
        m_MeshGenerator.transformUpdateDelegate += UpdateTransform;
    }

    void UpdateTransform(Vector3 localPosition, Quaternion localRotation)
    {
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
    }
}

class MeshGenerator
{
    private static MeshGenerator meshGenerator;

    private ROSConnection m_ROSConnection;
    private TFSystem m_TFSystem;
    private float m_LastDrawingFrameTime;

    private List<Vector3> vertexBuffer;
    private List<Vector2> uvBuffer;
    private List<int> triBuffer;


    public delegate void TransformUpdateDelegate(Vector3 localPosition, Quaternion localRotation);
    public TransformUpdateDelegate transformUpdateDelegate;

    public Mesh mesh { get; private set; }

    public static MeshGenerator GetOrCreateInstance()
    {
        if (meshGenerator != null)
        {
            return meshGenerator;
        }

        meshGenerator = new MeshGenerator();
        return meshGenerator;
    }

    private MeshGenerator()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_ROSConnection.Subscribe<OccupancyGridMsg>("/map", UpdateMap);

        m_TFSystem = TFSystem.GetOrCreateInstance();

        mesh = new Mesh();

        vertexBuffer = new List<Vector3>(201 * 201);
        uvBuffer = new List<Vector2>(201 * 201);
        triBuffer = new List<int>(200 * 200 * 6);
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
        MapMetaDataMsg info = msg.info;
        vertexBuffer.Clear();
        vertexBuffer.Capacity = ((int)info.width + 1) * ((int)info.height + 1);
        uvBuffer.Clear();
        uvBuffer.Capacity = ((int)info.width + 1) * ((int)info.height + 1);

        triBuffer.Clear();
        triBuffer.Capacity = ((int)info.width) * ((int)info.height) * 3 * 2;

        for (int y = 0; y <= info.height; y++)
        {
            for (int x = 0; x <= info.width; x++)
            {
                Vector3 vertex = new Vector3(x * info.resolution, 0, y * info.resolution);

                int threshold = 50;
                if (x < info.width && y < info.height && msg.data[x + info.width * y] > threshold // Check upper right of vertex
                    || (x > 0 && y < info.height && msg.data[x - 1 + info.width * y] > threshold) // Check upper left of vertex
                    || (x < info.width && y > 0 && msg.data[x + info.width * (y - 1)] > threshold) // Check lower right of vertex
                    || (x > 0 && y > 0 && msg.data[x - 1 + info.width * (y - 1)] > threshold)) // Check lower left of vertex
                {
                    vertex.y = 0.4f;
                }

                vertexBuffer.Add(vertex);
                uvBuffer.Add(new Vector2(x / info.width, 1 - y / info.height));

                if (y < info.height && x < info.width)
                {
                    triBuffer.Add((int)(x + y * (info.width + 1)));
                    triBuffer.Add((int)(x + (y + 1) * (info.width + 1)));
                    triBuffer.Add((int)(x + 1 + y * (info.width + 1)));

                    triBuffer.Add((int)(x + (y + 1) * (info.width + 1)));
                    triBuffer.Add((int)(x + 1 + (y + 1) * (info.width + 1)));
                    triBuffer.Add((int)(x + 1 + y * (info.width + 1)));
                }
            }
        }

        mesh.Clear();
        mesh.SetVertices(vertexBuffer);
        mesh.SetUVs(0, uvBuffer);
        mesh.SetTriangles(triBuffer, 0);
        mesh.RecalculateNormals();
    }

}
