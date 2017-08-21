using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public float speed = 20;

	void Update () {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");

        transform.position += transform.right * h + transform.forward * v * Time.deltaTime * speed;

        var x = Input.GetAxis("Mouse X");
        var y = Input.GetAxis("Mouse Y");

        transform.rotation *= Quaternion.Euler(0f, x, 0f);
        transform.rotation *= Quaternion.Euler(-y, 0f, 0f);
    }
}
