using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public float viewRange = 30;
    public Chunk chunkPrefab;
    public int chunkSize = 20;
    void Update()
    {

        for (float x = transform.position.x - viewRange; x < transform.position.x + viewRange; x += chunkSize)
        {
            for (float z = transform.position.z - viewRange; z < transform.position.z + viewRange; z += chunkSize)
            {

                Vector3 pos = new Vector3(x, 0, z);
                pos.x = Mathf.Floor(pos.x / (float)chunkSize) * chunkSize;
                pos.z = Mathf.Floor(pos.z / (float)chunkSize) * chunkSize;

                Chunk chunk = Chunk.GetChunk(pos);
                if (chunk != null) continue;

                chunk = (Chunk)Instantiate(chunkPrefab, pos, Quaternion.identity);



            }
        }


    }
}
