using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimplexNoise;

[RequireComponent (typeof(MeshRenderer))]
[RequireComponent (typeof(MeshCollider))]
[RequireComponent (typeof(MeshFilter))]
public class Chunk : MonoBehaviour {
	
	public static List<Chunk> chunks = new List<Chunk>();
    public int width;
    public int height;
	public int[,,] map;
    public int[,] heightMap;
	public Mesh chunkMesh;
	protected MeshRenderer meshRenderer;
	protected MeshCollider meshCollider;
	protected MeshFilter meshFilter;

    Vector3 offset;

    public int seed;
    float baseHeight = 10;


    public float frequency = 0.025f;
    public float amplitude = 1;
    public int GetByte(Vector3 worldPos)
    {
        worldPos -= transform.position;
        int x = Mathf.FloorToInt(worldPos.x);
        int y = Mathf.FloorToInt(worldPos.y);
        int z = Mathf.FloorToInt(worldPos.z);
        return GetByte(x, y, z);
    }

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
    }

    void Update()
    {
        Generate();
    }


    void Generate()
    {
        Random.InitState(seed);
        offset = new Vector3(Random.value * 1000, Random.value * 1000, Random.value * 1000);

        map = new int[width, height, width];
        heightMap = new int[width, width];
        GenerateHeightMap();
        GenerateMap();
        BuildChunk();
    }

    void GenerateMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    map[x, y, z] = CalculateType(new Vector3(x, y, z) + transform.position);
                }
            }
        }
    }

    void GenerateHeightMap()
    {
        for(int x = 0; x < width; x++)
        {
            for(int z = 0; z < width; z++)
            {
                heightMap[x, z] = CalculateHeight(new Vector3(x, 0f, z) + transform.position);
            }
        }
    }

    int CalculateHeight(Vector3 wPos)
    {
        float x = (wPos.x + offset.x) * frequency;
        float z = (wPos.z + offset.z) * frequency;

        float noise = Noise.Generate(x, z) * amplitude;

        return Mathf.FloorToInt(noise + baseHeight);
    }

    int CalculateType(Vector3 wPos)
    {
        //y坐标是否在Chunk内
        if (wPos.y < 0 || wPos.y >= height)
        {
            return 0;
        }

        float heightMapValue = CalculateHeight(wPos);

        if (wPos.y < 5)
        {
            return 2;
        }

        if (wPos.y == heightMapValue)
        {
            return 1;
        }
        else if (wPos.y < heightMapValue && wPos.y > heightMapValue - 5)
        {
            return 3;
        }
        else if (wPos.y > heightMapValue)
        {
            return 0;
        }
        else
        {
            return 5;
        }
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

        int brick = map[x, y, z];

        // Left
        if (CheckBuildFace(x - 1, y, z))
            BuildFace(brick, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris);
        // Right
        if (CheckBuildFace(x + 1, y, z))
            BuildFace(brick, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris);

        // Bottom
        if (CheckBuildFace(x, y - 1, z))
            BuildFace(brick, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris);
        // Top
        if (CheckBuildFace(x, y + 1, z))
            BuildFace(brick, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris);

        // Back
        if (CheckBuildFace(x, y, z - 1))
            BuildFace(brick, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris);
        // Front
        if (CheckBuildFace(x, y, z + 1))
            BuildFace(brick, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris);


    }

    void BuildFace(int brick, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris)
	{
        int index = verts.Count;
		
		verts.Add (corner);
		verts.Add (corner + up);
		verts.Add (corner + up + right);
		verts.Add (corner + right);
		
		Vector2 uvWidth = new Vector2(0.25f, 0.25f);
		Vector2 uvCorner = new Vector2(0.00f, 0.75f);

        uvCorner.x += (float)(brick - 1) / 4;
        uvCorner.y -= (float)Mathf.Floor(brick/4) * uvWidth.x;
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

    bool CheckBuildFace (int x, int y, int z)
	{
		if ( y < 0) return false;
		int brick = GetByte(x,y,z);
		switch (brick)
		{
		case 0: 
			return true;
		default:
			return false;
		}
	}

    int GetByte (int x, int y , int z) {
        //y坐标是否在Chunk内
		if ((y < 0) || (y >= height))
			return 0;

        //X/Z坐标是否在Chunk外
        if ((x < 0) || (z < 0) || (x >= width) || (z >= width))
        {
            return 0;

        }
        return map[x,y,z];
	}

	public bool SetBrick(byte brick, Vector3 worldPos){
		worldPos -= transform.position;
		return SetBrick (brick, Mathf.FloorToInt(worldPos.x),Mathf.FloorToInt(worldPos.y),Mathf.FloorToInt(worldPos.z));
	}

	public bool SetBrick(byte brick, int x, int y, int z){
		if ((x < 0) || (y < 0) || (z < 0) || (x >= width) || (y >= height || (z >= width))) {
			return false;
		}
		if (map [x, y, z] == brick) return false;
		map [x, y, z] = brick;
        BuildChunk();

		if (x == 0) {
			Chunk chunk = GetChunk(new Vector3(x - 2,y,z) + transform.position);
			if (chunk != null){
				chunk.BuildChunk();
			}
		}
		if (x == width - 1) {
			Chunk chunk = GetChunk(new Vector3(x + 2,y,z) + transform.position);
			if (chunk != null){
                chunk.BuildChunk();
			}
		}
		if (z == 0) {
			Chunk chunk = GetChunk(new Vector3(x,y,z - 2) + transform.position);
			if (chunk != null){
                chunk.BuildChunk();
			}
		}
		if (z == width - 1) {
			Chunk chunk = GetChunk(new Vector3(x,y,z + 2) + transform.position);
			if (chunk != null){
                chunk.BuildChunk();
			}
		}
		return true;
	}

}


