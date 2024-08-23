using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    private const int CHUNK_WIDTH = 16;
    private const int CHUNK_HEIGHT = 64;
    public Material material;
    private WorldBiomeSource mWorldBiomeSource;

    void Start()
    {
        // 4096 : 1
        mWorldBiomeSource = new WorldBiomeSource(114L);
        
        for (int x = 0; x < 32; x++)
        {
            for (int z = 0; z < 32; z++)
            {
                // 实际坐标：--o----o--，其中第一个o为原点，所以间距应该是width/2，但是采样点应该是不缩放的
                BuildChunk(new Vector3(x, 0, z));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position">原始位置</param>
    void BuildChunk(Vector3 position)
    {
        // 生成坐标
        Vector3 insPos = new Vector3(position.x * CHUNK_WIDTH * .5f, 0, position.z * CHUNK_WIDTH * .5f);
        // 采样坐标
        Vector3 texturePos = new Vector3(position.x * CHUNK_WIDTH, 0, position.z * CHUNK_WIDTH);

        // 为了让边缘面连续，这里多存两个方块采样，下标为0和CHUNK_WIDTH对应左右两个边界
        bool[,,] block = new bool[CHUNK_WIDTH + 2, CHUNK_WIDTH + 2, CHUNK_HEIGHT];
        for (int x = 0; x < CHUNK_WIDTH + 2; x++)
        {
            for (int z = 0; z < CHUNK_WIDTH + 2; z++)
            {
                for (int y = 0; y < CHUNK_HEIGHT; y++)
                {
                    float value = Mathf.PerlinNoise((x + texturePos.x) * .1f, (z + texturePos.z) * .1f) * 10 + y;
                    if (value <= (CHUNK_HEIGHT * .5f))
                    {
                        block[x, z, y] = true;
                    }
                }
            }
        }

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        // 1 和 CHUNK_WIDTH为另外CHUNK的值
        for (int x = 1; x <= CHUNK_WIDTH; x++)
        {
            for (int z = 1; z <= CHUNK_WIDTH; z++)
            {
                for (int y = 0; y < CHUNK_HEIGHT; y++)
                {
                    if (!block[x, z, y]) continue;

                    int visibility = 0;

                    // 0-front
                    if (z >= 1 && !block[x, z - 1, y])
                        visibility |= 0b000001;
                    // 1-back
                    if (z < CHUNK_WIDTH + 1 && !block[x, z + 1, y])
                        visibility |= 0b000010;
                    // 2-top
                    if (y < CHUNK_HEIGHT - 1 && !block[x, z, y + 1])
                        visibility |= 0b000100;
                    // 3-bottom
                    if (y > 0 && !block[x, z, y - 1])
                        visibility |= 0b001000;
                    // 4-left
                    if (x >= 1 && !block[x - 1, z, y])
                        visibility |= 0b010000;
                    // 5-right
                    if (x < CHUNK_WIDTH + 1 && !block[x + 1, z, y])
                        visibility |= 0b100000;

                    if (visibility != 0)
                    {
                        BuildMesh(new Vector3(x + insPos.x, y + insPos.y, z + insPos.z), visibility, vertices,
                            normals, triangles);
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();

        GameObject chunk = new GameObject("Chunk");
        chunk.AddComponent<MeshFilter>().mesh = mesh;
        chunk.AddComponent<MeshRenderer>().material = material;
        chunk.transform.position = insPos;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="visibility">低地址：0-front,back,top,bottom,left,right</param>
    /// <param name="vertices"></param>
    /// <param name="normals"></param>
    /// <param name="triangles"></param>
    void BuildMesh(Vector3 position, int visibility, List<Vector3> vertices, List<Vector3> normals, List<int> triangles)
    {
        int vertexCount = vertices.Count;

        if ((visibility & 0b1) == 0b1) // front
        {
            vertices.Add(position + new Vector3(0, 0, 0));
            vertices.Add(position + new Vector3(0, 1, 0));
            vertices.Add(position + new Vector3(1, 1, 0));
            vertices.Add(position + new Vector3(1, 0, 0));

            normals.Add(Vector3.back);
            normals.Add(Vector3.back);
            normals.Add(Vector3.back);
            normals.Add(Vector3.back);

            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);

            vertexCount += 4;
        }

        if ((visibility & 0b10) == 0b10) // back
        {
            vertices.Add(position + new Vector3(1, 0, 1));
            vertices.Add(position + new Vector3(1, 1, 1));
            vertices.Add(position + new Vector3(0, 1, 1));
            vertices.Add(position + new Vector3(0, 0, 1));

            normals.Add(Vector3.forward);
            normals.Add(Vector3.forward);
            normals.Add(Vector3.forward);
            normals.Add(Vector3.forward);

            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);

            vertexCount += 4;
        }

        if ((visibility & 0b100) == 0b100) // top
        {
            vertices.Add(position + new Vector3(0, 1, 0));
            vertices.Add(position + new Vector3(0, 1, 1));
            vertices.Add(position + new Vector3(1, 1, 1));
            vertices.Add(position + new Vector3(1, 1, 0));

            normals.Add(Vector3.up);
            normals.Add(Vector3.up);
            normals.Add(Vector3.up);
            normals.Add(Vector3.up);

            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);

            vertexCount += 4;
        }

        if ((visibility & 0b1000) == 0b1000) // bottom
        {
            vertices.Add(position + new Vector3(0, 0, 1));
            vertices.Add(position + new Vector3(0, 0, 0));
            vertices.Add(position + new Vector3(1, 0, 0));
            vertices.Add(position + new Vector3(1, 0, 1));

            normals.Add(Vector3.down);
            normals.Add(Vector3.down);
            normals.Add(Vector3.down);
            normals.Add(Vector3.down);

            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);

            vertexCount += 4;
        }

        if ((visibility & 0b10000) == 0b10000) // left
        {
            vertices.Add(position + new Vector3(0, 0, 1));
            vertices.Add(position + new Vector3(0, 1, 1));
            vertices.Add(position + new Vector3(0, 1, 0));
            vertices.Add(position + new Vector3(0, 0, 0));

            normals.Add(Vector3.left);
            normals.Add(Vector3.left);
            normals.Add(Vector3.left);
            normals.Add(Vector3.left);

            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);

            vertexCount += 4;
        }

        if ((visibility & 0b100000) == 0b100000) // right
        {
            vertices.Add(position + new Vector3(1, 0, 0));
            vertices.Add(position + new Vector3(1, 1, 0));
            vertices.Add(position + new Vector3(1, 1, 1));
            vertices.Add(position + new Vector3(1, 0, 1));

            normals.Add(Vector3.right);
            normals.Add(Vector3.right);
            normals.Add(Vector3.right);
            normals.Add(Vector3.right);

            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);

            vertexCount += 4;
        }
    }
}