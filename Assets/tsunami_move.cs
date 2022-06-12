using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tsunami_move : MonoBehaviour
{
    public float speed = 50.0f;

    // Update is called once per frame
    void Update()
    {
        bool temp = false;
        Vector3 move = new Vector3(0, 0, 0);
        float deltaTime = Time.deltaTime;

        if (Input.GetKey(KeyCode.F))
        {
            move = new Vector3(0, 0, -1) * deltaTime * speed;
            temp = true;
        }
        if (temp)
        {
            transform.Translate(move);
            transform.Rotate(Vector3.right, (speed/40));
        }


    }
}
