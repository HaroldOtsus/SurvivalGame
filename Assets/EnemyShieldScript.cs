using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyShieldScript : MonoBehaviour
{
    // Liikumise muutujad - Moving variables
    public Rigidbody2D enemyRigidbody2D;        // Füüsikaline komponent - Physical component
    public BoxCollider2D enemyBoxCollider2D;    // Kokkupõrgete tuvastamiseks - To detect collisions
    public float moveSpeed;                     // Vastase liikumis kiirus - Enemy character movement speed
    public LayerMask obstacleLayer;             // Defineerib ära takistused maailmas - Defines obstacles in the world
    private Transform player;                   // Mängija asukoht maailmas - Player location in the world

    // Vajalik NavMesh'il liikumiseks - For movement on the NavMesh
    NavMeshAgent agent;

    // Mängija poolt valitud algoritmi tüüp - Player chosen algorithm type
    public int algorithmtype;

    // Patrullimise muutujad - Patrolling variables
    public List<Transform> waypoints = new List<Transform>();       // Nimekiri sihtpunktide asukohtadest - List of waypoints' locations
    private int currentWaypointIndex = 0;                           // Määrab ära hetkese sihtpunkti
    private bool isWaiting = false;                                 // Tõsi, kui vastane ootab sihtpunktis - True, if enemy is waiting at a waypoint
    private float waitTimer = 0f;                                   // Määrab ära ootamisaja sihtpunktis - Determines the time waited at waypoint

    // Loogika muutujad - Logic Variables
    public LogicManagerScript logicManager;
    public SpawnManagerScript spawnManager;
    public bool enemyIsAlive = true;            // Tõsi, kui vastane on elus
    public int escapeHealthThreshold;           // Elu punktide lävend, milleni jõudes vastane põgeneb - When reached this health threshold, the enemy will try to escape
    private bool isEscaping = false;            // Tõsi, kui vastane põgeneb - True, if the enemy is escaping
    public GameObject enemyPrefab;              // Sisaldab püstoliga vastase prefab'i - Includes the pistol enemy prefab

    // Tuvastamise muutujad - Detection variables
    public float detectionRange;                // Kaugus, millal vastane tuvastab mängitava karakteri ja ründab teda - Distance when enemy will detect player and attack
    public float stoppingRange;                 // Kaugus, millal vastane jääb seisma mängijast - Distance when the enemy will stop movement from the player

    // Sprite'ide muutujad - Sprite variables
    public Sprite[] spriteArray;                // Massiiv liikumise sprite'idest - Array of movement sprites
    public Sprite deathSprite;                  // Hävitatud vastast kujutav sprite
    public SpriteRenderer spriteRenderer;
    private int spriteCount = 0;                // spriteArray massiivi indeks - Index for the spriteArray

    // Määrab ära, kui kiiresti sprite'id muutuvad, kui mängija liigub - Determines how fast the sprites will change when the character is in move
    public float spriteRate = 0.08f;
    private float spriteTimer;

    // Elu punktide muutujad - Health points variables
    public int maxHealth = 20;      // Maksimum elu punktid
    public int currentHealth;       // Hetkene elu punktide väärtus

    // Vigastuse muutujad - Damage variables
    // Vigastuse saamise kiirus, vajalik, et mängija ei saaks liiga palju vigastusi liiga kiiresti kurikate käest
    // Needed so the player would not take too much damage too fast from melee attacks
    public float damageSpeed;
    public float damageTimer;

    void Start()
    {
        algorithmtype = PlayerPrefs.GetInt("AlgorithmType");
        currentHealth = maxHealth;
        logicManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManagerScript>();
        spawnManager = FindObjectOfType<SpawnManagerScript>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // Kui algoritm on 2, siis leiab sihtpunktide mänguobjektid ja lisab nende asukohad nimekirja
        // If algorithm is 2, then finds all waypoint game objects and adds their locations in the list
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
            // Leiab kauguse ja suuna mängijani
            // Finds distance and direction to the player
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            Vector2 direction = player.position - transform.position;

            // Lihtsa algortimi puhul
            // 'Lihtne algoritm' option
            if (algorithmtype == 1)
            {
                LookAtPlayer();
                Movement();

                // Liigub mängija suunas
                // Moves toward the player location
                agent.SetDestination(player.position);
                agent.speed = moveSpeed;

                AttackPlayer(direction, distanceToPlayer);
            }
            // Oleku algoritmi puhul
            // 'Oleku algoritm' option
            else if (algorithmtype == 2)
            {
                agent.speed = moveSpeed;

                // Kontrollib mängija kaugust vastasest ja kui mängija on piisavalt lähedal vastasele, siis vastane tuvastab teda ning ründab
                // Checks player distance from the enemy character and if enemy is close enough to the player, then the enemy will attack
                if (Vector3.Distance(transform.position, player.position) < detectionRange)
                {
                    LookAtPlayer();
                    Movement();

                    // Liigub mängija suunas
                    // Moves toward the player location
                    agent.SetDestination(player.position);

                    AttackPlayer(direction, distanceToPlayer);
                }
                // Kui vastane ei ole piisavalt lähedal, et vastast tuvastada
                // If enemy is not close enough to detect player
                else
                {
                    // Kontrollib, kas vastane ootab sihtpunktis
                    // Checks if the enemy is waiting at a waypoint
                    if (isWaiting)
                    {
                        // Vähendab ootamis taimeri väärtust
                        // Decreases the timer variable value
                        waitTimer -= Time.deltaTime;
                        if (waitTimer <= 0f)
                        {
                            isWaiting = false;
                        }
                    }
                    // Kui vastane hetkel ei otsi teekonda sihtpunkti ja on sihtpunktile väga lähedal
                    // If enemy does not have a path pending and is close to the waypoint
                    else if (!agent.pathPending && agent.remainingDistance <= 0.5f)
                    {
                        // Kui vastane pannakse ootama sihtpunkti
                        // If enemy has to wait at a waypoint
                        if (Wait())
                        {
                            isWaiting = true;
                            waitTimer = 20f;
                            agent.isStopped = true;
                        }
                        // Kui ei panda ootama, siis antakse uus sihtpunkt
                        // If enemy does not have to wait, then enemy receives a new waypoint
                        else
                        {
                            agent.isStopped = false;
                            ChangeWaypoint();
                        }
                    }
                    // Kui vastane liigub sihtpunkti
                    // If enemy is moving towards a waypoint
                    else
                    {
                        Vector2 directionToWaypoint = waypoints[currentWaypointIndex].position - transform.position;
                        transform.right = directionToWaypoint.normalized;

                        Movement();

                        agent.SetDestination(waypoints[currentWaypointIndex].position);
                    }
                } 
            }
            // Adaptiivse algoritmi puhul
            // 'Adaptiivne algoritm' option
            else
            {
                LookAtPlayer();
                Movement();

                agent.speed = moveSpeed;

                // Leiab kõige lähedasema püstoliga vastase
                // Finds the closest pistol enemy
                GameObject closestEnemy = FindClosestEnemy(transform.position, 30f);

                if (closestEnemy != null)
                {
                    // Arvutab kauguse püstoliga vastaseni
                    // Calculates the distance to the pistol enemy
                    float distance = Vector3.Distance(transform.position, closestEnemy.transform.position);
                    
                    // Kontrollib, kas püstoliga vastane on piisavalt lähedal, et teda kaitsta
                    // Checks if the pistol enemy is close enough to protect them
                    if (distance < 30f)
                    {
                        // Liigub püstoliga vastase ette
                        // Moves in front of the pistol enemy
                        Vector3 targetPosition = closestEnemy.transform.position + closestEnemy.transform.right * 1.0f;
                        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

                        // Kui vastane on väga lähedal sihtpunktile
                        // If enemy is really close to the target
                        if (distanceToTarget < 0.05f)
                        {
                            // Sprite vahetus vajalik, kuna kui kilbiga vastane jääb seisma püstoliga vastase ees, siis ta peaks seisma jäämis
                            // sprite'ile vahetama, aga millegi pärast ta seda ei tee, siis järgnev koodirida teeb seda ise
                            // Sprite change needed, because once the shield enemy is close enough to the pistol enemy,
                            // then the sprite should change to the not moving sprite
                            spriteRenderer.sprite = spriteArray[0];
                            agent.isStopped = true;
                        }
                        // Kui ei ole nii lähedale, siis liigub uuesti püstoliga vastase ette
                        // If not very close, then moves again to be more in front of the pistol enemy
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
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                }
            }
        }

        // Kui vastane põgeneb ja on elus
        if (isEscaping && enemyIsAlive)
        {
            agent.speed = moveSpeed;
            agent.isStopped = false;

            // Määratakse suund sihtpunkti
            Vector2 directionToWaypoint = waypoints[currentWaypointIndex].position - transform.position;
            transform.right = directionToWaypoint.normalized;

            Movement();
            agent.SetDestination(waypoints[currentWaypointIndex].position);

            // Kui vastane on jõudnud sihtpunkti, siis enam ei põgene
            // If enemy has reached the waypoint, then enemy stops escaping
            if (!agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                isEscaping = false;
            }
        }
    }

    // Ründab mängijat
    private void AttackPlayer(Vector2 direction, float distanceToPlayer)
    {
        // Vajalik, et vastane näeks takistusi enda ees
        // So the enemy could see obstacles on its way
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 10f, obstacleLayer);
        if (hit)
        {
            // Kui näeb mängijat, siis jääb seisma
            // If enemy sees the player then enemy stops
            if (hit.collider.name == "Player")
            {
                if (distanceToPlayer <= stoppingRange)
                {
                    agent.isStopped = true;
                }
                else
                {
                    agent.isStopped = false;
                }
            }
            // Kui mängija ei ole, siis on takistus ja vastane peab liikuma mängija selja taha
            // Else it is an obstacle and the enemy needs to move behind the player
            else
            {
                agent.isStopped = false;
                agent.SetDestination(player.position - player.transform.forward);
            }
        }
    }

    // Leiab kõige lähedasema püstoliga vastase
    // Finds the closest pistol enemy
    private GameObject FindClosestEnemy(Vector3 position, float searchRadius)
    {
        // Leiab kõik püstoliga vastased ette antud raadiusega
        // Finds all pistol enemies in a given radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, searchRadius);

        GameObject closestEnemy = null;
        float closestDistance = 100000f;

        // Käib läbi kõik leitud vastased
        // Checks all found enemies
        foreach (var collider in colliders)
        {
            // Kontrollib, et vastane oleks püstoliga
            // Checks that the enemy has a pistol
            if (collider.CompareTag("EnemyTypePistol"))
            {
                // Arvutab kauguse vastaseni
                // Calculates the distance to the enemy
                float distance = Vector3.Distance(position, collider.transform.position);
                
                // Kui see on kõige lähim püstoliga vastane, siis salvestab selle
                // If it is the closest pistol enemy, then the pistol enemy is saved
                if (distance < closestDistance)
                {
                    closestEnemy = collider.gameObject;
                    closestDistance = distance;
                }
            }
        }
        
        return closestEnemy;
    }

    // Kui suvaline väärtus on alla 0.31-e siis tagastab 'true'
    // If random value is below 0.31 then returns true
    private bool Wait()
    {
        return Random.value < 0.5f;
    }

    // Määrab vastasele uue juhusliku sihtpunkti
    // Determines new random waypoint for the enemy
    private void ChangeWaypoint()
    {
        currentWaypointIndex = Random.Range(0, waypoints.Count);
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    // Suunab vastase vaatama mängija poole
    // Makes the enemy character face wherever the player is
    private void LookAtPlayer()
    {
        Vector2 direction = player.position - transform.position;

        transform.right = direction.normalized;
    }

    // Näitab vastase sprite, mis kujutavad liikumist
    // Makes the enemy character show movement sprites
    private void Movement()
    {
        // Kontrollib, kas vastane liigub
        // Checks if the enemy character is moving
        if (!agent.isStopped)
        {
            // Kontrollib spriteTimer'it, et sprite'id ei vahetuks liiga kiiresti
            // Checks the spriteTimer so that the sprites would not change too often
            if (spriteTimer <= 0f)
            {
                // Algväärtustab sprite'ide massiivi esimese elemendini, kui on jõutud viimase elemendini
                // Resets the sprite array back to the first element if spriteCount has reached the end of the array
                if (spriteCount > (spriteArray.Length - 1))
                {
                    spriteCount = 0;
                }

                // Muudab hetkest sprite'i ja inkrementeerib spriteCount loendurit
                // Sets the current sprite and increments the spriteCount
                spriteRenderer.sprite = spriteArray[spriteCount];
                spriteCount++;
                spriteTimer = spriteRate;

            }
            else
            {
                // Vähendab taimerit 
                // Decrease the timer variable
                spriteTimer -= Time.deltaTime;
            }
        }
    }
   

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Kui vastane saab pihta kuuliga, siis vähendatakse tema elu punkte
        // If enemy gets hit with a bullet, then their health points are decreased
        if (collision.gameObject.tag == "Bullet")
        {
            currentHealth -= 10;
        }

        // Oleku algoritmi puhul kontrollitakse elu punktide lävendit ja kui hetkene elu punktide väärtus on madalam sellest, siis vastane põgeneb
        // 'Oleku algoritm' option, enemy escapeHealthThreshold is checked to determine if the enemy has to start escaping or not
        if (algorithmtype == 2)
        {
            if (currentHealth <= escapeHealthThreshold)
            {
                isEscaping = true;
                ChangeWaypoint();
            }
        }

        // Kui vastase elu punktid on otsas, siis vastane hävitatakse
        if (currentHealth <= 0)
        {
            CharacterDestroy();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damageTimer <= 0f)
        {
            // Kui vastane saab pihta kurikaga, siis vähendatakse tema elu punkte
            // If enemy gets hit with a melee attack, then their health points are decreased
            if (collision.gameObject.tag == "Melee")
            {
                currentHealth -= 20;
            }

            // Oleku algoritmi puhul kontrollitakse elu punktide lävendit ja kui hetkene elu punktide väärtus on madalam sellest, siis vastane põgeneb
            // 'Oleku algoritm' option, enemy escapeHealthThreshold is checked to determine if the enemy has to start escaping or not
            if (algorithmtype == 2)
            {
                if (currentHealth <= escapeHealthThreshold)
                {
                    isEscaping = true;
                    ChangeWaypoint();
                }
            }

            // Kui vastase elu punktid on otsas, siis vastane hävitatakse
            if (currentHealth <= 0)
            {
                CharacterDestroy();
            }
            damageTimer = damageSpeed;
        }
        else
        {
            // Vähendab taimeri väärtusi
            // Decreases the timer variables
            damageTimer -= Time.deltaTime;
        }
    }

    // Vastase füüsilised mängu objektid hävitatakse ja määratakse vastase elus olemise tõeväärtuse valeks
    // Enemy physical game objects are destroyed
    private void CharacterDestroy()
    {
        spriteRenderer.sprite = deathSprite;
        Destroy(enemyRigidbody2D);
        Destroy(enemyBoxCollider2D);
        Destroy(agent);
        spriteRenderer.sortingLayerName = "Dead";
        enemyIsAlive = false;

        // Lisatakse punkte mängijale
        logicManager.addScore(1);
        spawnManager.enemyShieldDestroyedCount++;

        // Ilmutatakse uus püstoliga vastane
        // Pistol enemy is spawned in
        Instantiate(enemyPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}
