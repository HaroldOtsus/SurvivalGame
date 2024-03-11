using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class EnemyScript : MonoBehaviour
{
    // Moving variables
    public Rigidbody2D enemyRigidbody2D;
    public BoxCollider2D enemyBoxCollider2D;
    public float moveSpeed;
    public float moveTimer = 1;
    public float stoppingDistance = 3f;
    public float avoidDistance = 2f;
    public LayerMask obstacleLayer;
    private Transform player;

    NavMeshAgent agent;

    // Logic Variables
    public LogicManagerScript logicManager;
    public bool enemyIsAlive = true;

    // Shooting variables
    public GameObject bulletPrefab;
    public Transform firingPoint;
    public float fireRate = 2f;                 // Determines how fast the bullets will shoot out
    public float fireRateTimer;
    public float shootingSpriteRate = 0.25f;    // Determines how long the shooting sprite will stay as the displayed sprite
    private float shootingSpriteTimer;
    public float detectionRange = 5f;
    public float shootingRange = 5f;

    // Sprite variables
    public Sprite[] spriteArray;
    public Sprite shootingSprite;
    public Sprite deathSprite;
    public SpriteRenderer spriteRenderer;
    private int spriteCount = 0;                // Initialized to '0' to show the first sprite in the spriteArray
    public float spriteRate = 0.08f;            // Determines how fast the sprites will change when the character is in move
    private float spriteTimer;

    // Health variables
    public int maxHealth = 20;
    public int currentHealth;
    private bool hasDied = false;

    public float damageSpeed;
    public float damageTimer;

    void Start()
    {
        currentHealth = maxHealth;
        logicManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManagerScript>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        if (enemyIsAlive)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            Vector2 direction = player.position - transform.position;

            LookAtPlayer();
            Movement(distanceToPlayer);

            agent.SetDestination(player.position);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 10f, obstacleLayer);
            if (hit)
            {
                if (hit.collider.name == "Player")
                {
                    if (distanceToPlayer <= shootingRange)
                    {
                        agent.isStopped = true;
                        Shooting();
                    }
                    else
                    {
                        agent.isStopped = false;
                    }
                }
                else
                {
                    agent.SetDestination(player.position - player.transform.forward);
                }
            }
        }
        else if (!hasDied)
        {
            logicManager.addScore(15);
            Invoke("DestroyObject", 60f);
            hasDied = true;
        }
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }

    // Makes the enemy character face wherever the player is
    private void LookAtPlayer()
    {
        Vector2 direction = player.position - transform.position;

        transform.right = direction.normalized;
    }

    // Makes the enemy character move
    private void Movement(float distanceToPlayer)
    {
        // Checks if the enemy character is moving
        if (!agent.isStopped)
        {
            // Checks the spriteTimer so that the sprites would not change too often
            // Checks the shootingSpriteTimer so that the shooting sprite would show for a little longer
            if (spriteTimer <= 0f && shootingSpriteTimer <= 0f)
            {
                // Resets the sprite list back to the first element if spriteCount has reached the end of the array
                if (spriteCount > (spriteArray.Length - 1))
                {
                    spriteCount = 0;
                }

                // Sets the current sprite and increments the spriteCount
                // spriteTimer variable is reset so the current sprite would show for a little longer
                spriteRenderer.sprite = spriteArray[spriteCount];
                spriteCount++;
                spriteTimer = spriteRate;

            }
            else
            {
                // Decrease the timer variable
                spriteTimer -= Time.deltaTime;
            }
        }
        else
        {
            // If character is no longer shooting then show the base not moving sprite
            if (shootingSpriteTimer <= 0f)
            {
                spriteRenderer.sprite = spriteArray[0];
            }
        }
    }
   

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            currentHealth -= 10;
        }
        if (currentHealth <= 0)
        {
            spriteRenderer.sprite = deathSprite;
            Destroy(enemyRigidbody2D);
            Destroy(enemyBoxCollider2D);
            Destroy(agent);
            spriteRenderer.sortingLayerName = "Dead";
            enemyIsAlive = false;
        }
    }

    // Makes the enemy character shoot and changes the character sprite to a shooting sprite when mouse1 button is pressed
    private void Shooting()
    {
        if (fireRateTimer <= 0f)
        {
            // Spawns the bullet and sets fire rate
            Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation);
            fireRateTimer = fireRate;

            spriteRenderer.sprite = shootingSprite;
            shootingSpriteTimer = shootingSpriteRate;
        }
        else
        {
            // Decrease the timer variables
            if (!(shootingSpriteTimer <= 0f))
            {
                shootingSpriteTimer -= Time.deltaTime;
            }
            fireRateTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damageTimer <= 0f)
        {
            if (collision.gameObject.tag == "Melee")
            {
                currentHealth -= 20;
            }
            if (currentHealth <= 0)
            {
                spriteRenderer.sprite = deathSprite;
                Destroy(enemyRigidbody2D);
                Destroy(enemyBoxCollider2D);
                spriteRenderer.sortingLayerName = "Dead";
                enemyIsAlive = false;
            }
            damageTimer = damageSpeed;
        }
        else
        {
            damageTimer -= Time.deltaTime;
        }
    }
}
