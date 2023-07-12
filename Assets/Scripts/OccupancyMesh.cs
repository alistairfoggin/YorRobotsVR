using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using RosMessageTypes.Std;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class OccupancyMesh : MonoBehaviour
{
    ROSConnection m_ROSConnection;
    TFSystem m_TFSystem;

    static sbyte[] grid = new sbyte[] { 0, 0,
                                        0, 0 };

    static OccupancyGridMsg staticMsg = new OccupancyGridMsg(new HeaderMsg(),
        new MapMetaDataMsg(
        new TimeMsg(), 1f, 2, 2, new PoseMsg()), grid);

    Mesh m_Mesh;

    List<Vector3> vertexBuffer;
    List<Vector2> uvBuffer;
    List<int> triBuffer;

    private OccupancyGridMsg m_Message;
    private float m_LastDrawingFrameTime;
    [SerializeField]
    private float wallHeight = 0.4f;

    // Start is called before the first frame update
    void Start()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_ROSConnection.Subscribe<OccupancyGridMsg>("/map", UpdateMap);

        m_TFSystem = TFSystem.GetOrCreateInstance();

        m_Mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = m_Mesh;

        vertexBuffer = new List<Vector3>(201 * 201);
        uvBuffer = new List<Vector2>(201 * 201);
        triBuffer = new List<int>(200 * 200 * 6);
    }

    void UpdateMap(OccupancyGridMsg occupancyGridMsg)
    {
        m_Message = occupancyGridMsg;

        if (Time.time > m_LastDrawingFrameTime + 1)
            Redraw();

        m_LastDrawingFrameTime = Time.time;
    }

    void Redraw()
    {
        GenerateMesh();

        Vector3 origin = m_Message.info.origin.position.From<FLU>();
        Quaternion rotation = m_Message.info.origin.orientation.From<FLU>();
        rotation.eulerAngles += new Vector3(0, -90, 0);
        float scale = m_Message.info.resolution;

        // offset the mesh by half a grid square, because the message's position defines the CENTER of grid square 0,0
        Vector3 drawOrigin = origin - rotation * new Vector3(scale * 0.5f, 0, scale * 0.5f);

        TFFrame tfFrame = m_TFSystem.GetTransform(m_Message.header);
        drawOrigin = tfFrame.TransformPoint(drawOrigin);
        rotation = Quaternion.Euler(rotation.eulerAngles + tfFrame.rotation.eulerAngles);

        transform.localPosition = drawOrigin;
        transform.localRotation = rotation;
    }

    void GenerateMesh()
    {
        MapMetaDataMsg info = m_Message.info;
        vertexBuffer.Clear();
        vertexBuffer.Capacity = ((int)info.width + 1) * ((int)info.height + 1);
        uvBuffer.Clear();
        uvBuffer.Capacity = ((int)info.width + 1) * ((int)info.height + 1);

        for (int y = 0; y <= info.height; y++)
        {
            for (int x = 0; x <= info.width; x++)
            {
                Vector3 vertex = new Vector3(x * info.resolution, 0, y * info.resolution);

                int threshold = 50;
                if (x < info.width && y < info.height && m_Message.data[x + info.width * y] > threshold // Check upper right of vertex
                    || (x > 0 && y < info.height && m_Message.data[x - 1 + info.width * y] > threshold) // Check upper left of vertex
                    || (x < info.width && y > 0 && m_Message.data[x + info.width * (y - 1)] > threshold) // Check lower right of vertex
                    || (x > 0 && y > 0 && m_Message.data[x - 1 + info.width * (y - 1)] > threshold)) // Check lower left of vertex
                {
                    vertex.y = wallHeight;
                }

                vertexBuffer.Add(vertex);
                uvBuffer.Add(new Vector2(x / info.width, 1 - y / info.height));
            }
        }

        triBuffer.Clear();
        triBuffer.Capacity = ((int)info.width) * ((int)info.height) * 3 * 2;
        for (int row = 0; row < info.height; row++)
        {
            for (int col = 0; col < info.width; col++)
            {
                triBuffer.Add((int)(col + row * (info.width + 1)));
                triBuffer.Add((int)(col + (row + 1) * (info.width + 1)));
                triBuffer.Add((int)(col + 1 + row * (info.width + 1)));

                triBuffer.Add((int)(col + (row + 1) * (info.width + 1)));
                triBuffer.Add((int)(col + 1 + (row + 1) * (info.width + 1)));
                triBuffer.Add((int)(col + 1 + row * (info.width + 1)));
            }
        }

        m_Mesh.Clear();
        m_Mesh.SetVertices(vertexBuffer);
        m_Mesh.SetUVs(0, uvBuffer);
        m_Mesh.SetTriangles(triBuffer, 0);
        m_Mesh.RecalculateNormals();
    }
}
