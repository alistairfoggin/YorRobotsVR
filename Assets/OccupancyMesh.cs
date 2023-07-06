using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Nav;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using RosMessageTypes.BuiltinInterfaces;

public class OccupancyMesh : MonoBehaviour
{
    static sbyte[] grid = new sbyte[] { 0, 1, 
                                        0, 0 };

    static OccupancyGridMsg occupancyMsg = new OccupancyGridMsg(new HeaderMsg(),
        new MapMetaDataMsg(
        new TimeMsg(), 1f, 2, 2, new PoseMsg()), grid);

    Mesh m_Mesh;

    // Start is called before the first frame update
    void Start()
    {
        m_Mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = m_Mesh;

        GenerateMesh(m_Mesh, occupancyMsg);

    }

    void GenerateMesh(Mesh mesh, OccupancyGridMsg occupancyGridMsg)
    {
        MapMetaDataMsg info = occupancyGridMsg.info;
        Vector3[] vertices = new Vector3[(info.height + 1) * (info.height + 1)];
        Vector2[] uv= new Vector2[(info.height + 1) * (info.height + 1)];

        int numSquares = 0;
        for (int row = 0; row <= info.height; row++)
        {
            for (int col = 0; col <= info.width; col++)
            {
                vertices[col + info.width * row] = new Vector3(col * info.resolution, 0, -row * info.resolution);
                uv[col + info.width * row] = new Vector2(col / info.width, 1 - row / info.height);
                if (row < info.height && col < info.width && occupancyGridMsg.data[col + info.width * row] > 0)
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
                if (occupancyGridMsg.data[col + info.width * row] > 0)
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

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }
}
