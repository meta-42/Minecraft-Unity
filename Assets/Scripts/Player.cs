using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    CharacterController cc;
    public float speed = 20;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update () {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");

       

        var x = Input.GetAxis("Mouse X");
        var y = Input.GetAxis("Mouse Y");

        transform.rotation *= Quaternion.Euler(0f, x, 0f);
        transform.rotation *= Quaternion.Euler(-y, 0f, 0f);

        if (Input.GetButton("Jump"))
        {
            cc.Move((transform.right * h + transform.forward * v + transform.up )* speed * Time.deltaTime);
        }
        else
        {
            cc.SimpleMove(transform.right * h + transform.forward * v * speed);
        }
    }
}
