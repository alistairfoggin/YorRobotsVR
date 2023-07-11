using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Nav;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using RosMessageTypes.BuiltinInterfaces;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

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

    Texture2D m_Texture;
    private OccupancyGridMsg m_Message;
    bool m_TextureIsDirty = true;
    [SerializeField]
    Material m_Material;
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
        GetComponent<MeshRenderer>().material = m_Material;
        GetComponent<MeshCollider>().sharedMesh = m_Mesh;

        vertexBuffer = new List<Vector3>(201 * 201);
        uvBuffer = new List<Vector2>(201 * 201);
        triBuffer = new List<int>(200 * 200 * 6);

        //Generate2DMesh();

        //m_Message = staticMsg;
        //GenerateMesh();

    }

    void UpdateMap(OccupancyGridMsg occupancyGridMsg)
    {
        m_Message = occupancyGridMsg;
        m_TextureIsDirty = true;

        if (Time.time > m_LastDrawingFrameTime + 1)
            //Redraw2D();
            Redraw();

        m_LastDrawingFrameTime = Time.time;
    }

    public void Redraw2D()
    {
        m_Material.mainTexture = GetTexture();

        Vector3 origin = m_Message.info.origin.position.From<FLU>();
        Quaternion rotation = m_Message.info.origin.orientation.From<FLU>();
        rotation.eulerAngles += new Vector3(0, -90, 0); // TODO: Account for differing texture origin
        float scale = m_Message.info.resolution;

        // offset the mesh by half a grid square, because the message's position defines the CENTER of grid square 0,0
        Vector3 drawOrigin = origin - rotation * new Vector3(scale * 0.5f, 0, scale * 0.5f);

        TFFrame tfFrame = m_TFSystem.GetTransform(m_Message.header);
        drawOrigin = tfFrame.TransformPoint(drawOrigin);
        rotation = Quaternion.Euler(rotation.eulerAngles + tfFrame.rotation.eulerAngles);

        transform.localPosition = drawOrigin;
        transform.localRotation = rotation;
        transform.localScale = new Vector3(m_Message.info.width * scale, 1, m_Message.info.height * scale);
    }

    void Generate2DMesh()
    {
        m_Mesh.vertices = new[]
        { Vector3.zero, new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 0) };
        m_Mesh.uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
        m_Mesh.triangles = new[] { 0, 1, 2, 2, 3, 0 };       
    }

    // From OccupancyGridDefaultVisualizer
    public Texture2D GetTexture()
    {
        if (!m_TextureIsDirty)
            return m_Texture;

        if (m_Texture == null)
        {
            m_Texture = new Texture2D((int)m_Message.info.width, (int)m_Message.info.height, TextureFormat.R8, true);
            m_Texture.wrapMode = TextureWrapMode.Clamp;
            m_Texture.filterMode = FilterMode.Point;
        }
        else if (m_Message.info.width != m_Texture.width || m_Message.info.height != m_Texture.height)
        {
            m_Texture.Reinitialize((int)m_Message.info.width, (int)m_Message.info.height);
        }

        m_Texture.SetPixelData(m_Message.data, 0);
        m_Texture.Apply();
        m_TextureIsDirty = false;
        return m_Texture;
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
        //Vector3[] vertices = new Vector3[(info.width + 1) * (info.height + 1)];
        //Vector2[] uv = new Vector2[(info.width + 1) * (info.height + 1)];

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
