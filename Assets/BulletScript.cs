using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Rigidbody2D bulletRigidBody2D;
    private float bulletSpeed = 20f;
    private float bulletLifetime = 3f;

    // Start is called before the first frame update
    void Start()
    {
        bulletRigidBody2D = GetComponent<Rigidbody2D>();
        Destroy(gameObject, bulletLifetime);
    }

    // Update is called once per frame
    void Update()
    {
        bulletRigidBody2D.velocity = transform.up * bulletSpeed;
    }
}
