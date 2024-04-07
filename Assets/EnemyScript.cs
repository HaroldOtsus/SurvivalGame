using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
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

    // Tulistamise muutujad - Shooting variables
    public GameObject bulletPrefab;             // Kuuli prefab m�nguobjekt
    public Transform firingPoint;               // Asukoht, kust kuul v�lja lendab - Point where the bullet spawns from
    public float fireRate;                      // Tulistamis kiirus - Determines how fast the bullets will shoot out
    public float fireRateTimer;
    public float detectionRange;                // Kaugus, millal vastane tuvastab m�ngitava karakteri ja r�ndab teda - Distance when enemy will detect player and attack
    public float shootingRange;                 // Kaugus, millal vastane hakkab m�ngijat tulistama - Distance when the enemy will shoot at the player

    // Tulistamise sprite'i n�itamise aeg sekundites - Determines how long the shooting sprite will stay as the displayed sprite(in seconds)
    public float shootingSpriteRate = 0.25f;    
    private float shootingSpriteTimer;

    // Sprite'ide muutujad - Sprite variables
    public Sprite[] spriteArray;                // Massiiv liikumise sprite'idest - Array of movement sprites
    public Sprite shootingSprite;               // Tulistamist kujutav sprite
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

    // K�lgsuunalise liikumise muutujad - Strafing variables
    private bool isStrafing = false;        // T�si, kui vastane kasutab k�lgsuunalist liikumist
    private float strafeTimer = 0f;         // K�lgsuunalise liikumise taimer
    private float strafeDuration = 2f;      // K�lgsuunalise liikumise kestus sekundites
    private float strafeDistance = 2f;      // K�lgsuunalise liikumise kaugus
    private Vector3 strafeDirection;        // K�lgsuunalise liikumise suund

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

                // Kui m�ngija liigub vastasele liiga l�hedale, siis vastane �ritab selg ees eemalduda m�ngijast
                // If the player moves too close to the enemy, the enemy will move backwards away from the player
                if (distanceToPlayer <= 4.0f)
                {
                    agent.isStopped = false;
                    agent.speed = moveSpeed;
                    Shooting();

                    // Liigub m�ngijast eemale tagurpidi
                    // Move away from the player backwards
                    Vector3 moveAwayDirection = -direction.normalized; 
                    Vector3 targetPosition = transform.position + moveAwayDirection * 5.0f;

                    agent.SetDestination(targetPosition);
                }
                // Kui m�ngija on vastasest kaugemal
                // If the player is not near the enemy
                else
                {
                    // Liigub m�ngija suunas
                    // Moves toward the player location
                    agent.SetDestination(player.position);
                    agent.speed = moveSpeed;

                    // Vajalik, et vastane n�eks takistusi enda ees
                    // So the enemy could see obstacles on its way
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 10f, obstacleLayer);
                    if (hit)
                    {
                        // Kui n�eb m�ngijat, siis j��b seisma ja tulistab ning lisaks kasutab k�lgsuunalist liikumist
                        // If enemy sees the player then enemy stops and starts to shoot at the player and also strafes
                        if (hit.collider.name == "Player")
                        {
                            if (distanceToPlayer <= shootingRange)
                            {
                                agent.isStopped = true;
                                Shooting();

                                // Kontrollib k�lgsuunalise liikumise taimeri v��rtust, kui on v�hemv�rdne kui null, siis teeb uue k�lgsuunalise liikumise
                                // Checks strafing timer, if it is less or equal than 0, then enemy starts a new strafe
                                if (strafeTimer <= 0f)
                                {
                                    isStrafing = true;

                                    // Valib suvaliselt kas vasaku v�i parema suuna
                                    // Chooses randomly either the left or right direction
                                    int randomValue = Random.Range(0, 2) == 0 ? 90 : -90;

                                    strafeDirection = Quaternion.Euler(0, 0, randomValue) * direction.normalized;

                                    strafeTimer = strafeDuration;
                                }
                                else
                                {
                                    // V�hendab taimeri v��rtust
                                    // Decreases the timer value
                                    strafeTimer -= Time.deltaTime;
                                }

                                // Kui vastane liigub k�lgsuunaliselt
                                if (isStrafing)
                                {
                                    agent.isStopped = false;

                                    // Arvutab positsiooni kuhu k�lgsuunaliselt liikuda
                                    // Calculate the target position for strafing
                                    Vector3 strafeTargetPosition = transform.position + strafeDirection * strafeDistance;

                                    // M��rab asukoha kuhu k�lgsuunaliselt liikuda
                                    // Set the destination for strafing
                                    agent.SetDestination(strafeTargetPosition);
                                }
                            }
                            else
                            {
                                isStrafing = false;
                                agent.isStopped = false;
                            }
                        }
                        else
                        {
                            isStrafing = false;
                            agent.isStopped = false;
                            agent.SetDestination(player.position - player.transform.forward);
                        }
                    }
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
            // Kui n�eb m�ngijat, siis j��b seisma ja tulistab
            // If enemy sees the player then enemy stops and starts to shoot at the player
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
            // Kui m�ngija ei ole, siis on takistus ja vastane peab liikuma m�ngija selja taha
            // Else it is an obstacle and the enemy needs to move behind the player
            else
            {
                agent.isStopped = false;
                agent.SetDestination(player.position - player.transform.forward);
            }
        }
    }
    
    // Kui suvaline v��rtus on alla 0.31-e siis tagastab 'true'
    // If random value is below 0.31 then returns true
    private bool Wait()
    {
        return Random.value < 0.31f;
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
            // Kontrollib shootingSpriteTimer'it, et tulistamise sprite p�siks natuke kauem n�htaval
            // Checks the shootingSpriteTimer so that the shooting sprite would show for a little longer
            if (spriteTimer <= 0f && shootingSpriteTimer <= 0f)
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
        else
        {
            // Kui vastane enam ei tulista, siis n�ita algset liikumis sprite'i
            // If enemy is no longer shooting then show the base not moving sprite
            if (shootingSpriteTimer <= 0f)
            {
                spriteRenderer.sprite = spriteArray[0];
            }
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
        spawnManager.enemyPistolDestroyedCount++;
        Invoke("DestroyObject", 60f);
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

    // Vastane tulistab m�ngija suunas ja muudab sprite'i tulistamise sprite'iks
    // Makes the enemy character shoot and changes the sprite to a shooting sprite
    private void Shooting()
    {
        if (fireRateTimer <= 0f)
        {
            // Ilmutab kuuli ja m��rab tulistamise kiiruse
            // Spawns the bullet and sets fire rate
            Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation);
            fireRateTimer = fireRate;

            spriteRenderer.sprite = shootingSprite;
            shootingSpriteTimer = shootingSpriteRate;
        }
        else
        {
            // V�hendab taimeri v��rtusi
            // Decreases the timer variables
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

    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}
