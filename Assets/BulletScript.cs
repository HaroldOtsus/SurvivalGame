using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Rigidbody2D bulletRigidBody2D;
    private float bulletSpeed = 20f;
    private float bulletLifetime = 0.5f;
    void Start()
    {
        bulletRigidBody2D = GetComponent<Rigidbody2D>();
        Destroy(gameObject, bulletLifetime);
    }
    void Update()
    {
        bulletRigidBody2D.velocity = transform.up * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag != "Enemy")
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag == "Enemy" && gameObject.tag == "Bullet")
        {
            Destroy(gameObject);
        }
    }
}
