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

    public int algorithmtype;

    // Waypoints for patrolling
    public List<Transform> waypoints = new List<Transform>();
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    // Logic Variables
    public LogicManagerScript logicManager;
    public bool enemyIsAlive = true;
    public int escapeHealthThreshold;
    private bool isEscaping = false;

    // Shooting variables
    public GameObject bulletPrefab;
    public Transform firingPoint;
    public float fireRate       ;               // Determines how fast the bullets will shoot out
    public float fireRateTimer;
    public float shootingSpriteRate = 0.25f;    // Determines how long the shooting sprite will stay as the displayed sprite
    private float shootingSpriteTimer;
    public float detectionRange;
    public float shootingRange;

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

    public float damageSpeed;
    public float damageTimer;

    void Start()
    {
        algorithmtype = PlayerPrefs.GetInt("AlgorithmType");
        currentHealth = maxHealth;
        logicManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManagerScript>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (algorithmtype == 2)
        {
            GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
            foreach (GameObject waypointObject in waypointObjects)
            {
                waypoints.Add(waypointObject.transform);
            }
            ChangeWaypoint();
        }
}

    void Update()
    {
        if (enemyIsAlive && !isEscaping)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            Vector2 direction = player.position - transform.position;
            if (algorithmtype == 1)
            {
                LookAtPlayer();
                Movement();

                agent.SetDestination(player.position);
                agent.speed = moveSpeed;

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
            else if (algorithmtype == 2)
            {
                agent.speed = moveSpeed;
                if (Vector3.Distance(transform.position, player.position) < detectionRange)
                {
                    LookAtPlayer();
                    Movement();
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
                else
                {
                    if (isWaiting)
                    {
                        waitTimer -= Time.deltaTime;
                        if (waitTimer <= 0f)
                        {
                            isWaiting = false;
                        }
                    }
                    else if (!agent.pathPending && agent.remainingDistance <= 0.5f)
                    {
                        if (Wait())
                        {
                            isWaiting = true;
                            waitTimer = 20f;
                            agent.isStopped = true;
                        }
                        else
                        {
                            agent.isStopped = false;
                            ChangeWaypoint();
                        }
                    }
                    else
                    {
                        Vector2 directionToWaypoint = waypoints[currentWaypointIndex].position - transform.position;
                        transform.right = directionToWaypoint.normalized;
                        
                        Movement();

                        agent.SetDestination(waypoints[currentWaypointIndex].position);
                    }
                    
                }
                
            }
            else
            {
                LookAtPlayer();
                Movement();

                agent.SetDestination(player.position);
                agent.speed = moveSpeed;

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
        }

        if (isEscaping && enemyIsAlive)
        {
            agent.speed = moveSpeed;
            agent.isStopped = false;

            Vector2 directionToWaypoint = waypoints[currentWaypointIndex].position - transform.position;
            transform.right = directionToWaypoint.normalized;

            Movement();
            agent.SetDestination(waypoints[currentWaypointIndex].position);

            if (!agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                isEscaping = false;
            }
        }
    }


    private bool Wait()
    {
        return Random.value < 0.5f;
    }

    private void ChangeWaypoint()
    {
        currentWaypointIndex = Random.Range(0, waypoints.Count);
        agent.SetDestination(waypoints[currentWaypointIndex].position);
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
    private void Movement()
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

        if (algorithmtype == 2)
        {
            if (currentHealth <= escapeHealthThreshold)
            {
                isEscaping = true;
                ChangeWaypoint();
            }
        }
        
        if (currentHealth <= 0)
        {
            spriteRenderer.sprite = deathSprite;
            Destroy(enemyRigidbody2D);
            Destroy(enemyBoxCollider2D);
            Destroy(agent);
            spriteRenderer.sortingLayerName = "Dead";
            enemyIsAlive = false;
            logicManager.addScore(1);
            Invoke("DestroyObject", 60f);
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

            if (algorithmtype == 2)
            {
                if (currentHealth <= escapeHealthThreshold)
                {
                    isEscaping = true;
                    ChangeWaypoint();
                }
            }

            if (currentHealth <= 0)
            {
                spriteRenderer.sprite = deathSprite;
                Destroy(enemyRigidbody2D);
                Destroy(enemyBoxCollider2D);
                Destroy(agent);
                spriteRenderer.sortingLayerName = "Dead";
                enemyIsAlive = false;
                logicManager.addScore(1);
                Invoke("DestroyObject", 60f);
            }
            damageTimer = damageSpeed;
        }
        else
        {
            damageTimer -= Time.deltaTime;
        }
    }
}
