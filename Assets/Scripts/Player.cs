using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    CharacterController cc;
    public float speed = 20;
    public float viewRange = 30;
    public Chunk chunkPrefab;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update ()
    {
        UpdateInput();
        UpdateWorld();
    }

    void UpdateInput()
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");

        var x = Input.GetAxis("Mouse X");
        var y = Input.GetAxis("Mouse Y");

        transform.rotation *= Quaternion.Euler(0f, x, 0f);
        transform.rotation *= Quaternion.Euler(-y, 0f, 0f);

        if (Input.GetButton("Jump"))
        {
            cc.Move((transform.right * h + transform.forward * v + transform.up) * speed * Time.deltaTime);
        }
        else
        {
            cc.SimpleMove(transform.right * h + transform.forward * v * speed);
        }
    }

    void UpdateWorld()
    {
        for (float x = transform.position.x - viewRange; x < transform.position.x + viewRange; x += Chunk.width)
        {
            for (float z = transform.position.z - viewRange; z < transform.position.z + viewRange; z += Chunk.width)
            {
                Vector3 pos = new Vector3(x, 0, z);
                pos.x = Mathf.Floor(pos.x / (float)Chunk.width) * Chunk.width;
                pos.z = Mathf.Floor(pos.z / (float)Chunk.width) * Chunk.width;

                Chunk chunk = Chunk.GetChunk(pos);
                if (chunk != null) continue;

                chunk = (Chunk)Instantiate(chunkPrefab, pos, Quaternion.identity);
            }
        }
    }
}
