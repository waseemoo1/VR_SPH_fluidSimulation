using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cube : MonoBehaviour
{

    public float upForce = 1f;
    public float sideForce = 1f;

    // Start is called before the first frame update
    void Start()
    {
        float xForce = Random.Range(-sideForce,sideForce);
        float yForce = Random.Range(upForce ,upForce/ 2f);
        float zForce = Random.Range(-sideForce,sideForce);

        Vector3 force = new Vector3(xForce,yForce,0);

        GetComponent<Rigidbody>().velocity = force;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
