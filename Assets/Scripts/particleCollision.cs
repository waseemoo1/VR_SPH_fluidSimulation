using UnityEngine;

public class particleCollision : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        print("here"); 
        print(collision.gameObject);
    }
    
}
