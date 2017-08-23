using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimplexNoise;

public enum BlockType
{
    None = 0,
    Dirt = 1,
    Grass = 3,
    Gravel = 4,
}

[RequireComponent (typeof(MeshRenderer))]
[RequireComponent (typeof(MeshCollider))]
[RequireComponent (typeof(MeshFilter))]
public class Chunk : MonoBehaviour {
	
	public static List<Chunk> chunks = new List<Chunk>();
    public static int width = 30;
    public static int height = 30;

    public int seed;
    public float baseHeight = 10;
    public float frequency = 0.025f;
    public float amplitude = 1;

    BlockType[,,] map;
    Mesh chunkMesh;
	MeshRenderer meshRenderer;
	MeshCollider meshCollider;
	MeshFilter meshFilter;

    Vector3 offset0;
    Vector3 offset1;
    Vector3 offset2;

    public static Chunk GetChunk(Vector3 wPos)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            Vector3 tempPos = chunks[i].transform.position;

            //wPos是否超出了Chunk的XZ平面的范围
            if ((wPos.x < tempPos.x) || (wPos.z < tempPos.z) || (wPos.x >= tempPos.x + 20) || (wPos.z >= tempPos.z + 20))
                continue;

            return chunks[i];
        }
        return null;
    }

    
    void Start () {
		
		chunks.Add(this);
		
		meshRenderer = GetComponent<MeshRenderer>();
		meshCollider = GetComponent<MeshCollider>();
		meshFilter = GetComponent<MeshFilter>();
        Init();
    }

    void Init()
    {
        Random.InitState(seed);
        offset0 = new Vector3(Random.value * 1000, Random.value * 1000, Random.value * 1000);
        offset1 = new Vector3(Random.value * 1000, Random.value * 1000, Random.value * 1000);
        offset2 = new Vector3(Random.value * 1000, Random.value * 1000, Random.value * 1000);
        map = new BlockType[width, height, width];
        InitMap();
        BuildChunk();
    }

    void InitMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    map[x, y, z] = GenerateBlockType(new Vector3(x, y, z) + transform.position);
                }
            }
        }
    }

    int GenerateHeight(Vector3 wPos)
    {
        float x0 = (wPos.x + offset0.x) * frequency;
        float y0 = (wPos.y + offset0.y) * frequency;
        float z0 = (wPos.z + offset0.z) * frequency;

        float x1 = (wPos.x + offset1.x) * frequency * 2;
        float y1 = (wPos.y + offset1.y) * frequency * 2;
        float z1 = (wPos.z + offset1.z) * frequency * 2;

        float x2 = (wPos.x + offset2.x) * frequency / 4;
        float y2 = (wPos.y + offset2.y) * frequency / 4;
        float z2 = (wPos.z + offset2.z) * frequency / 4;

        float noise0 = Noise.Generate(x0, z0, y0) * amplitude;
        float noise1 = Noise.Generate(x1, z1, y1) * amplitude / 2;
        float noise2 = Noise.Generate(x2, z2, y2) * amplitude / 4;

        return Mathf.FloorToInt(noise0 + noise1 + noise2 + baseHeight);
    }

    BlockType GenerateBlockType(Vector3 wPos)
    {
        //y坐标是否在Chunk内
        if (wPos.y >= height)
        {
            return BlockType.None;
        }

        float genHeight = GenerateHeight(wPos);
        if (wPos.y > genHeight)
        {
            return BlockType.None;
        }
        else if (wPos.y == genHeight)
        {
            return BlockType.Grass;
        }
        else if (wPos.y < genHeight && wPos.y > genHeight - 5)
        {
            return BlockType.Dirt;
        }

        return BlockType.Gravel;
    }

	public void BuildChunk()
    {
		chunkMesh = new Mesh();
		List<Vector3> verts = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> tris = new List<int>();
		
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				for (int z = 0; z < width; z++)
				{
                    BuildBlock(x, y, z, verts, uvs, tris);
                }
			}
		}
					
		chunkMesh.vertices = verts.ToArray();
		chunkMesh.uv = uvs.ToArray();
		chunkMesh.triangles = tris.ToArray();
		chunkMesh.RecalculateBounds();
		chunkMesh.RecalculateNormals();
		
		meshFilter.mesh = chunkMesh;
		meshCollider.sharedMesh = chunkMesh;
	}

    void BuildBlock(int x, int y, int z, List<Vector3> verts, List<Vector2> uvs, List<int> tris)
    {
        if (map[x, y, z] == 0) return;

        BlockType brick = map[x, y, z];

        //Left
        if (CheckNeedBuildFace(x - 1, y, z))
            BuildFace(brick, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris);
        //Right
        if (CheckNeedBuildFace(x + 1, y, z))
            BuildFace(brick, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris);

        //Bottom
        if (CheckNeedBuildFace(x, y - 1, z))
            BuildFace(brick, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris);
        //Top
        if (CheckNeedBuildFace(x, y + 1, z))
            BuildFace(brick, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris);

        //Back
        if (CheckNeedBuildFace(x, y, z - 1))
            BuildFace(brick, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris);
        //Front
        if (CheckNeedBuildFace(x, y, z + 1))
            BuildFace(brick, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris);


    }

    void BuildFace(BlockType brick, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris)
	{
        int index = verts.Count;
		
		verts.Add (corner);
		verts.Add (corner + up);
		verts.Add (corner + up + right);
		verts.Add (corner + right);
		
		Vector2 uvWidth = new Vector2(0.25f, 0.25f);
		Vector2 uvCorner = new Vector2(0.00f, 0.75f);

        uvCorner.x += (float)(brick - 1) / 4;
        uvs.Add(uvCorner);
		uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
		uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
		uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
		
		if (reversed)
		{
			tris.Add(index + 0);
			tris.Add(index + 1);
			tris.Add(index + 2);
			tris.Add(index + 2);
			tris.Add(index + 3);
			tris.Add(index + 0);
		}
		else
		{
			tris.Add(index + 1);
			tris.Add(index + 0);
			tris.Add(index + 2);
			tris.Add(index + 3);
			tris.Add(index + 2);
			tris.Add(index + 0);
		}
		
	}

    bool CheckNeedBuildFace (int x, int y, int z)
	{
		if ( y < 0) return false;
		var type = GetBlockType(x,y,z);
		switch (type)
		{
		case BlockType.None: 
			return true;
		default:
			return false;
		}
	}

    public BlockType GetBlockType(int x, int y, int z)
    {
        if (y < 0 || y > height - 1)
        {
            return 0;
        }

        if ((x < 0) || (z < 0) || (x >= width) || (z >= width))
        {
            var id = GenerateBlockType(new Vector3(x, y, z) + transform.position);
            return id;
        }
        return map[x,y,z];
    }
}


