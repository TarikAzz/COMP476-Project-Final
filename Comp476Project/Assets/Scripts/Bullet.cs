using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _damage;

    public void Initialize(float damage, float velocity)
    {
        _damage = damage;
        GetComponent<Rigidbody>().velocity = transform.forward * velocity;
    }

    void OnTriggerEnter(Collider other)
    {
        var character = other.gameObject.GetComponent<Character>();
        if (character != null)
        {
            character.TakeDamage(_damage);
        }

        Destroy(gameObject);
    }
}
