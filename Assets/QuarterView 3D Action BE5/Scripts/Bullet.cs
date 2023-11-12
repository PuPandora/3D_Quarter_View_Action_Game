using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    public bool isMelee;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            Destroy(gameObject, 3);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isMelee)
        {
            return;
        }
            
        if (other.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
