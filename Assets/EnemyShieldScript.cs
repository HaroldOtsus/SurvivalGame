using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class EnemyShieldScript : MonoBehaviour
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
    public GameObject enemyPrefab;
    public int escapeHealthThreshold;
    private bool isEscaping = false;

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

                agent.speed = moveSpeed;

                GameObject closestEnemy = FindClosestEnemy(transform.position, 30f);

                if (closestEnemy != null)
                {
                    // Arvutab kauguse püstoliga vastaseni
                    float distance = Vector3.Distance(transform.position, closestEnemy.transform.position);
                    
                    // Kontrollib, kas püstoliga vastane on piisavalt lähedal, et teda kaitsta
                    if (distance < 30f)
                    {
                        // Liigub püstoliga vastase ette
                        Vector3 targetPosition = closestEnemy.transform.position + closestEnemy.transform.right * 2f;
                        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

                        if (distanceToTarget < 0.05f)
                        {
                            //Sprite vahetus vajalik, kuna kui kilbiga vastane jääb seisma püstoliga vastase ees, siis ta peaks seisma jäämis
                            //sprite'ile vahetama, aga millegi pärast ta seda ei tee, siis järgnev koodirida teeb seda ise
                            spriteRenderer.sprite = spriteArray[0];
                            agent.isStopped = true;
                        }
                        else
                        {
                            agent.isStopped = false;
                            agent.SetDestination(targetPosition);
                        }

                        LookAtPlayer();
                    }
                }
                else
                {
                    agent.SetDestination(player.position);
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

    private GameObject FindClosestEnemy(Vector3 position, float searchRadius)
    {
        // Leiab kõik püstoliga vastased ette antud raadiusega
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, searchRadius);

        GameObject closestEnemy = null;
        float closestDistance = 1000f;

        // Käib läbi kõik leitud vastased
        foreach (var collider in colliders)
        {
            // Kontrollib, et vastane oleks püstoliga
            if (collider.CompareTag("EnemyTypePistol"))
            {
                // Arvutab kauguse vastaseni
                float distance = Vector3.Distance(position, collider.transform.position);
                
                // Kui see on kõige lähim püstoliga vastane, siis salvestab selle
                if (distance < closestDistance)
                {
                    closestEnemy = collider.gameObject;
                    closestDistance = distance;
                }
            }
        }
        
        return closestEnemy;
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
            if (spriteTimer <= 0f)
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
            Instantiate(enemyPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
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
                Instantiate(enemyPrefab, transform.position, transform.rotation);
                Destroy(gameObject);
            }
            damageTimer = damageSpeed;
        }
        else
        {
            damageTimer -= Time.deltaTime;
        }
    }
}
