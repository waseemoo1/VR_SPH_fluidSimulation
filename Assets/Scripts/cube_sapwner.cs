using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cube_sapwner : MonoBehaviour
{
    public int limit=100;
    int counter=15;
    public int timer=15;
    public GameObject cubePrefab;

    // Update is called once per frame
    void FixedUpdate()
    {
        timer++;
        if(timer>15&&counter<limit)
        {
            Instantiate(cubePrefab,transform.position, Quaternion.identity);
            timer=0;
            counter++;
        }
    }
}
