using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyShieldScript : MonoBehaviour
{
    // Liikumise muutujad - Moving variables
    public Rigidbody2D enemyRigidbody2D;        // F��sikaline komponent - Physical component
    public BoxCollider2D enemyBoxCollider2D;    // Kokkup�rgete tuvastamiseks - To detect collisions
    public float moveSpeed;                     // Vastase liikumis kiirus - Enemy character movement speed
    public LayerMask obstacleLayer;             // Defineerib �ra takistused maailmas - Defines obstacles in the world
    private Transform player;                   // M�ngija asukoht maailmas - Player location in the world

    // Vajalik NavMesh'il liikumiseks - For movement on the NavMesh
    NavMeshAgent agent;

    // M�ngija poolt valitud algoritmi t��p - Player chosen algorithm type
    public int algorithmtype;

    // Patrullimise muutujad - Patrolling variables
    public List<Transform> waypoints = new List<Transform>();       // Nimekiri sihtpunktide asukohtadest - List of waypoints' locations
    private int currentWaypointIndex = 0;                           // M��rab �ra hetkese sihtpunkti
    private bool isWaiting = false;                                 // T�si, kui vastane ootab sihtpunktis - True, if enemy is waiting at a waypoint
    private float waitTimer = 0f;                                   // M��rab �ra ootamisaja sihtpunktis - Determines the time waited at waypoint

    // Loogika muutujad - Logic Variables
    public LogicManagerScript logicManager;
    public SpawnManagerScript spawnManager;
    public bool enemyIsAlive = true;            // T�si, kui vastane on elus
    public int escapeHealthThreshold;           // Elu punktide l�vend, milleni j�udes vastane p�geneb - When reached this health threshold, the enemy will try to escape
    private bool isEscaping = false;            // T�si, kui vastane p�geneb - True, if the enemy is escaping
    public GameObject enemyPrefab;              // Sisaldab p�stoliga vastase prefab'i - Includes the pistol enemy prefab

    // Tuvastamise muutujad - Detection variables
    public float detectionRange;                // Kaugus, millal vastane tuvastab m�ngitava karakteri ja r�ndab teda - Distance when enemy will detect player and attack
    public float stoppingRange;                 // Kaugus, millal vastane j��b seisma m�ngijast - Distance when the enemy will stop movement from the player

    // Sprite'ide muutujad - Sprite variables
    public Sprite[] spriteArray;                // Massiiv liikumise sprite'idest - Array of movement sprites
    public Sprite deathSprite;                  // H�vitatud vastast kujutav sprite
    public SpriteRenderer spriteRenderer;
    private int spriteCount = 0;                // spriteArray massiivi indeks - Index for the spriteArray

    // M��rab �ra, kui kiiresti sprite'id muutuvad, kui m�ngija liigub - Determines how fast the sprites will change when the character is in move
    public float spriteRate = 0.08f;
    private float spriteTimer;

    // Elu punktide muutujad - Health points variables
    public int maxHealth = 20;      // Maksimum elu punktid
    public int currentHealth;       // Hetkene elu punktide v��rtus

    // Vigastuse muutujad - Damage variables
    // Vigastuse saamise kiirus, vajalik, et m�ngija ei saaks liiga palju vigastusi liiga kiiresti kurikate k�est
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

        // Kui algoritm on 2, siis leiab sihtpunktide m�nguobjektid ja lisab nende asukohad nimekirja
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
            // Leiab kauguse ja suuna m�ngijani
            // Finds distance and direction to the player
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            Vector2 direction = player.position - transform.position;

            // Lihtsa algortimi puhul
            // 'Lihtne algoritm' option
            if (algorithmtype == 1)
            {
                LookAtPlayer();
                Movement();

                // Liigub m�ngija suunas
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

                // Kontrollib m�ngija kaugust vastasest ja kui m�ngija on piisavalt l�hedal vastasele, siis vastane tuvastab teda ning r�ndab
                // Checks player distance from the enemy character and if enemy is close enough to the player, then the enemy will attack
                if (Vector3.Distance(transform.position, player.position) < detectionRange)
                {
                    LookAtPlayer();
                    Movement();

                    // Liigub m�ngija suunas
                    // Moves toward the player location
                    agent.SetDestination(player.position);

                    AttackPlayer(direction, distanceToPlayer);
                }
                // Kui vastane ei ole piisavalt l�hedal, et vastast tuvastada
                // If enemy is not close enough to detect player
                else
                {
                    // Kontrollib, kas vastane ootab sihtpunktis
                    // Checks if the enemy is waiting at a waypoint
                    if (isWaiting)
                    {
                        // V�hendab ootamis taimeri v��rtust
                        // Decreases the timer variable value
                        waitTimer -= Time.deltaTime;
                        if (waitTimer <= 0f)
                        {
                            isWaiting = false;
                        }
                    }
                    // Kui vastane hetkel ei otsi teekonda sihtpunkti ja on sihtpunktile v�ga l�hedal
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

                // Leiab k�ige l�hedasema p�stoliga vastase
                // Finds the closest pistol enemy
                GameObject closestEnemy = FindClosestEnemy(transform.position, 30f);

                if (closestEnemy != null)
                {
                    // Arvutab kauguse p�stoliga vastaseni
                    // Calculates the distance to the pistol enemy
                    float distance = Vector3.Distance(transform.position, closestEnemy.transform.position);
                    
                    // Kontrollib, kas p�stoliga vastane on piisavalt l�hedal, et teda kaitsta
                    // Checks if the pistol enemy is close enough to protect them
                    if (distance < 30f)
                    {
                        // Liigub p�stoliga vastase ette
                        // Moves in front of the pistol enemy
                        Vector3 targetPosition = closestEnemy.transform.position + closestEnemy.transform.right * 1.0f;
                        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

                        // Kui vastane on v�ga l�hedal sihtpunktile
                        // If enemy is really close to the target
                        if (distanceToTarget < 0.05f)
                        {
                            // Sprite vahetus vajalik, kuna kui kilbiga vastane j��b seisma p�stoliga vastase ees, siis ta peaks seisma j��mis
                            // sprite'ile vahetama, aga millegi p�rast ta seda ei tee, siis j�rgnev koodirida teeb seda ise
                            // Sprite change needed, because once the shield enemy is close enough to the pistol enemy,
                            // then the sprite should change to the not moving sprite
                            spriteRenderer.sprite = spriteArray[0];
                            agent.isStopped = true;
                        }
                        // Kui ei ole nii l�hedale, siis liigub uuesti p�stoliga vastase ette
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

        // Kui vastane p�geneb ja on elus
        if (isEscaping && enemyIsAlive)
        {
            agent.speed = moveSpeed;
            agent.isStopped = false;

            // M��ratakse suund sihtpunkti
            Vector2 directionToWaypoint = waypoints[currentWaypointIndex].position - transform.position;
            transform.right = directionToWaypoint.normalized;

            Movement();
            agent.SetDestination(waypoints[currentWaypointIndex].position);

            // Kui vastane on j�udnud sihtpunkti, siis enam ei p�gene
            // If enemy has reached the waypoint, then enemy stops escaping
            if (!agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                isEscaping = false;
            }
        }
    }

    // R�ndab m�ngijat
    private void AttackPlayer(Vector2 direction, float distanceToPlayer)
    {
        // Vajalik, et vastane n�eks takistusi enda ees
        // So the enemy could see obstacles on its way
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 10f, obstacleLayer);
        if (hit)
        {
            // Kui n�eb m�ngijat, siis j��b seisma
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
            // Kui m�ngija ei ole, siis on takistus ja vastane peab liikuma m�ngija selja taha
            // Else it is an obstacle and the enemy needs to move behind the player
            else
            {
                agent.isStopped = false;
                agent.SetDestination(player.position - player.transform.forward);
            }
        }
    }

    // Leiab k�ige l�hedasema p�stoliga vastase
    // Finds the closest pistol enemy
    private GameObject FindClosestEnemy(Vector3 position, float searchRadius)
    {
        // Leiab k�ik p�stoliga vastased ette antud raadiusega
        // Finds all pistol enemies in a given radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, searchRadius);

        GameObject closestEnemy = null;
        float closestDistance = 100000f;

        // K�ib l�bi k�ik leitud vastased
        // Checks all found enemies
        foreach (var collider in colliders)
        {
            // Kontrollib, et vastane oleks p�stoliga
            // Checks that the enemy has a pistol
            if (collider.CompareTag("EnemyTypePistol"))
            {
                // Arvutab kauguse vastaseni
                // Calculates the distance to the enemy
                float distance = Vector3.Distance(position, collider.transform.position);
                
                // Kui see on k�ige l�him p�stoliga vastane, siis salvestab selle
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

    // Kui suvaline v��rtus on alla 0.31-e siis tagastab 'true'
    // If random value is below 0.31 then returns true
    private bool Wait()
    {
        return Random.value < 0.5f;
    }

    // M��rab vastasele uue juhusliku sihtpunkti
    // Determines new random waypoint for the enemy
    private void ChangeWaypoint()
    {
        currentWaypointIndex = Random.Range(0, waypoints.Count);
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    // Suunab vastase vaatama m�ngija poole
    // Makes the enemy character face wherever the player is
    private void LookAtPlayer()
    {
        Vector2 direction = player.position - transform.position;

        transform.right = direction.normalized;
    }

    // N�itab vastase sprite, mis kujutavad liikumist
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
                // Algv��rtustab sprite'ide massiivi esimese elemendini, kui on j�utud viimase elemendini
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
                // V�hendab taimerit 
                // Decrease the timer variable
                spriteTimer -= Time.deltaTime;
            }
        }
    }
   

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Kui vastane saab pihta kuuliga, siis v�hendatakse tema elu punkte
        // If enemy gets hit with a bullet, then their health points are decreased
        if (collision.gameObject.tag == "Bullet")
        {
            currentHealth -= 10;
        }

        // Oleku algoritmi puhul kontrollitakse elu punktide l�vendit ja kui hetkene elu punktide v��rtus on madalam sellest, siis vastane p�geneb
        // 'Oleku algoritm' option, enemy escapeHealthThreshold is checked to determine if the enemy has to start escaping or not
        if (algorithmtype == 2)
        {
            if (currentHealth <= escapeHealthThreshold)
            {
                isEscaping = true;
                ChangeWaypoint();
            }
        }

        // Kui vastase elu punktid on otsas, siis vastane h�vitatakse
        if (currentHealth <= 0)
        {
            CharacterDestroy();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damageTimer <= 0f)
        {
            // Kui vastane saab pihta kurikaga, siis v�hendatakse tema elu punkte
            // If enemy gets hit with a melee attack, then their health points are decreased
            if (collision.gameObject.tag == "Melee")
            {
                currentHealth -= 20;
            }

            // Oleku algoritmi puhul kontrollitakse elu punktide l�vendit ja kui hetkene elu punktide v��rtus on madalam sellest, siis vastane p�geneb
            // 'Oleku algoritm' option, enemy escapeHealthThreshold is checked to determine if the enemy has to start escaping or not
            if (algorithmtype == 2)
            {
                if (currentHealth <= escapeHealthThreshold)
                {
                    isEscaping = true;
                    ChangeWaypoint();
                }
            }

            // Kui vastase elu punktid on otsas, siis vastane h�vitatakse
            if (currentHealth <= 0)
            {
                CharacterDestroy();
            }
            damageTimer = damageSpeed;
        }
        else
        {
            // V�hendab taimeri v��rtusi
            // Decreases the timer variables
            damageTimer -= Time.deltaTime;
        }
    }

    // Vastase f��silised m�ngu objektid h�vitatakse ja m��ratakse vastase elus olemise t�ev��rtuse valeks
    // Enemy physical game objects are destroyed
    private void CharacterDestroy()
    {
        spriteRenderer.sprite = deathSprite;
        Destroy(enemyRigidbody2D);
        Destroy(enemyBoxCollider2D);
        Destroy(agent);
        spriteRenderer.sortingLayerName = "Dead";
        enemyIsAlive = false;

        // Lisatakse punkte m�ngijale
        logicManager.addScore(1);
        spawnManager.enemyShieldDestroyedCount++;

        // Ilmutatakse uus p�stoliga vastane
        // Pistol enemy is spawned in
        Instantiate(enemyPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}
