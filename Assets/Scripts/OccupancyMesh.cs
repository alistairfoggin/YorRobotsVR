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

    static sbyte[] grid = new sbyte[] { 0, 1, 
                                        0, 0 };

    static OccupancyGridMsg staticMsg = new OccupancyGridMsg(new HeaderMsg(),
        new MapMetaDataMsg(
        new TimeMsg(), 1f, 2, 2, new PoseMsg()), grid);

    Mesh m_Mesh;
    Texture2D m_Texture;
    private OccupancyGridMsg m_Message;
    bool m_TextureIsDirty = true;
    [SerializeField]
    Material m_Material;
    private float m_LastDrawingFrameTime;

    // Start is called before the first frame update
    void Start()
    {
        m_ROSConnection = ROSConnection.GetOrCreateInstance();
        m_ROSConnection.Subscribe<OccupancyGridMsg>("/map", UpdateMap);

        m_Mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = m_Mesh;
        GetComponent<MeshRenderer>().material = m_Material;
        GetComponent<MeshCollider>().sharedMesh = m_Mesh;

        Generate2DMesh();

        //GenerateMesh();

    }

    void UpdateMap(OccupancyGridMsg occupancyGridMsg)
    {
        m_Message = (OccupancyGridMsg) occupancyGridMsg;
        m_TextureIsDirty = true;

        if (Time.time > m_LastDrawingFrameTime)
            Redraw2D();

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

    void GenerateMesh()
    {
        MapMetaDataMsg info = m_Message.info;
        Vector3[] vertices = new Vector3[(info.height + 1) * (info.height + 1)];
        Vector2[] uv= new Vector2[(info.height + 1) * (info.height + 1)];

        int numSquares = 0;
        for (int row = 0; row <= info.height; row++)
        {
            for (int col = 0; col <= info.width; col++)
            {
                vertices[col + info.width * row] = new Vector3(col * info.resolution, 0, -row * info.resolution);
                uv[col + info.width * row] = new Vector2(col / info.width, 1 - row / info.height);
                if (row < info.height && col < info.width && m_Message.data[col + info.width * row] > 0)
                {
                    numSquares++;
                }
                print(vertices[col + info.width * row]);
            }
        }
        print(numSquares);
        int[] triangles = new int[numSquares * 2 * 3];
        int i = 0;
        for (int row = 0; row < info.height; row++)
        {
            for (int col = 0; col < info.width; col++)
            {
                if (m_Message.data[col + info.width * row] > 0)
                {
                    // TODO: update triangles to work
                    triangles[0 + i*6] = (int)(col + row * (info.width+1));
                    triangles[1 + i*6] = (int)(col + 1 + (row + 1) * (info.width+1));
                    triangles[2 + i*6] = (int)(col + 1 + row * (info.width + 1));
                    triangles[3 + i*6] = (int)(col + row * (info.width + 1));
                    triangles[4 + i*6] = (int)(col + (row + 1) * (info.width + 1));
                    triangles[5 + i*6] = (int)(col + 1 + (row + 1) * (info.width + 1));

                    print(triangles[0 + i * 6]);
                    print(triangles[1 + i * 6]);
                    print(triangles[2 + i * 6]);
                    print(triangles[3 + i * 6]);
                    print(triangles[4 + i * 6]);
                    print(triangles[5 + i * 6]);
                    i++;
                }
            }
        }

        m_Mesh.vertices = vertices;
        m_Mesh.uv = uv;
        m_Mesh.triangles = triangles;
    }
}
