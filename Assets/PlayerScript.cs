using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
    // Kasutajaliidese muutujad - UI variables
    public TextMeshProUGUI healthPoints;        // Elu punktide näitamiseks
    public TextMeshProUGUI shieldPoints;        // Kilbi punktide näitamiseks
    public Image equippedWeaponImage;           // Kasutuses oleva relva pilt
    public Sprite[] weaponSprites;              // Relvade piltide massiiv
    public Sprite shieldSprite;                 // Kilbi pilt

    // Hetkel kasutuses oleva relva näitamiseks - For displaying current weapon
    private int equippedWeaponIndex = 1;
    public GameObject gunObject;                // Relva objekt

    // Liikumise muutujad - Moving variables
    public Rigidbody2D playerRigidbody2D;       // Füüsikaline komponent - Physical component
    public BoxCollider2D playerBoxCollider2D;   // Kokkupõrgete tuvastamiseks - To detect collisions
    public float moveSpeed;                     // Mängitava karakteri kiirus - Playable character movement speed
    private Vector2 moveInput;                  // Kasutaja sisestus karakteri liikutamiseks - User input to move character

    // Loogika muutujad - Logic Variables
    public LogicManagerScript logicManager;
    public bool playerIsAlive = true;           // Tõsi, kui mängitav karakter on elus

    // Tulistamise muutujad - Shooting variables
    public GameObject bulletPrefab;             // Kuuli prefab mänguobjekt
    public Transform firingPoint;               // Asukoht, kust kuul välja lendab - Point where the bullet spawns from
    public float fireRate = 2f;                 // Tulistamis kiirus - Determines how fast the bullets will shoot out
    public float fireRateTimer;

    // Tulistamise sprite'i näitamise aeg sekundites - Determines how long the shooting sprite will stay as the displayed sprite(in seconds)
    public float shootingSpriteRate = 0.25f;    
    private float shootingSpriteTimer;          
    
    // Mängitava karakteri visuaalsete spraitide muutujad - Character sprite variables
    public Sprite[] spriteArray;                // Massiiv karakteri sprite'idest
    public Sprite shootingSprite;               // Tulistamist kujutav sprite
    public Sprite deathSprite;                  // Lüüa saanud mängitava karakteri sprite
    public SpriteRenderer spriteRenderer;       // Kuvab sprite
    private int spriteCount = 0;                // 0, et näidata esimest sprite'i massiivis - Initialized to '0' to show the first sprite in the spriteArray

    // Määrab, kui kiiresti sprite'id vahetuvad liikuval karakteril - Determines how fast the sprites will change when the character is in move
    public float spriteRate = 0.08f;            
    private float spriteTimer;

    // Elude muutujad - Health variables
    public int maxHealth;                   // Maksimum elu punktid - Maximum health points
    public int currentHealth;               // Hetke elu punktid
    public int shieldHealth;                // Kilbi punktid

    // Lähivõitluse muutujad - Melee variables
    public GameObject meleeWeapon;          // Lähivõitluse relva mängu objekt
    public Animator anim;                   // Animatsioonide jaoks
    public float meleeSpeed;                // Lähivõitluse löögi kiirus
    public float meleeTimer;

    // Vigastuse muutujad - Damage variables
    // Vigastuse saamise kiirus, vajalik, et mängija ei saaks liiga palju vigastusi liiga kiiresti kurikate käest
    // Needed so the player would not take too much damage too fast from melee attacks
    public float damageSpeed;               
    public float damageTimer;

    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "GameScene")
        {
            currentHealth = maxHealth;
            shieldHealth = maxHealth;
            healthPoints.text = "ELU PUNKTID: " + currentHealth.ToString();
            shieldPoints.text = "KILBI PUNKTID: " + shieldHealth.ToString();
            logicManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManagerScript>();
        }
        equippedWeaponImage.sprite = weaponSprites[0];
    }

    void Update()
    {
        // Mäng läbi, kui mängija hävitatakse
        // Game over if the player has been destroyed
        if (!playerIsAlive)
        {
            float delayInSeconds = 5f;
            StartCoroutine(LoadGameOverScene(delayInSeconds));
        }

        LookAtMouse();
        Movement();

        // 'Tab' nupuga saab relvasi vahetada
        // 'Tab' key changes current weapon
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            shootingSpriteTimer = 0;

            // Käib läbi järjest kõik elemendid kuni viimase elemendini
            // Cycles through all elements until the last element
            if (equippedWeaponIndex < weaponSprites.Length)
            {
                equippedWeaponImage.sprite = weaponSprites[equippedWeaponIndex];
                equippedWeaponIndex++;           
            }
            else
            {
                equippedWeaponIndex = 0;
                equippedWeaponImage.sprite = weaponSprites[equippedWeaponIndex];
                equippedWeaponIndex++;
            }
        }

        SpriteRenderer gunSprite = gunObject.GetComponent<SpriteRenderer>();
        if (equippedWeaponIndex == 1)
        {
            // Peidab kurika
            // Hides the melee weapon
            meleeWeapon.SetActive(false);

            // Peidab kilbi
            // Hides the shield
            gunSprite.sprite = null;

            Shooting();
        }
        else if (equippedWeaponIndex == 2)
        {
            // Toob kurika nähtavale
            // Displays the melee weapon
            meleeWeapon.SetActive(true);

            Melee();
        }
        else if (equippedWeaponIndex == 3 && shieldHealth > 0)
        {
            // Peidab kurika
            // Hides the melee weapon
            meleeWeapon.SetActive(false);

            // Kuvab kilbi
            // Displays the shield
            gunSprite.sprite = shieldSprite;
        }
    }
    private IEnumerator LoadGameOverScene(float delay)
    {
        // Oota etteantud viide sekundites
        // Waits for the given delay amount in seconds
        yield return new WaitForSeconds(delay);

        // Laadi mäng läbi stseen
        // Loads the game over scene
        SceneManager.LoadScene("GameOverScene");
    }

    private void ShieldCheck(Collider2D collision)
    {
        // Kui kasutaja saab pihta vastase kurikaga või kuuliga
        // If the player gets hit with enemy melee attacks or bullets
        if (collision.gameObject.tag == "EnemyMelee")
        {
            // Kui kilp on kasutusel, siis kilbi punktid vähenevad
            // If the shield is in use, then the shield points decrease
            if (equippedWeaponIndex == 3 && shieldHealth > 0)
            {
                shieldHealth -= 5;
                shieldPoints.text = "KILBI PUNKTID: " + shieldHealth.ToString();

                // Kui kilp on kasutusel, siis mängija saab elu punkte rünnakute eest
                // If the shield is used, then the player gets health points back against attacks
                currentHealth += 5;
                healthPoints.text = "ELU PUNKTID: " + currentHealth.ToString();
            }
            // Kui kilp ei ole kasutusel või kui kilbi punkte on 0 või vähem, siis vähenevad elu punktid
            // If the shield is not used or shield points are 0 or less, then health points are decreased
            else
            {
                currentHealth -= 5;
                healthPoints.text = "ELU PUNKTID: " + currentHealth.ToString();
            }
        }
    }

    private void ShieldCheck(Collision2D collision)
    {
        // Kui kasutaja saab pihta vastase kurikaga või kuuliga
        // If the player gets hit with enemy melee attacks or bullets
        if (collision.gameObject.tag == "EnemyBullet")
        {
            // Kui kilp on kasutusel, siis kilbi punktid vähenevad
            // If the shield is in use, then the shield points decrease
            if (equippedWeaponIndex == 3 && shieldHealth > 0)
            {
                shieldHealth -= 5;
                shieldPoints.text = "KILBI PUNKTID: " + shieldHealth.ToString();

                // Kui kilp on kasutusel, siis mängija saab elu punkte rünnakute eest
                // If the shield is used, then the player gets health points back against attacks
                currentHealth += 5;
                healthPoints.text = "ELU PUNKTID: " + currentHealth.ToString();
            }
            // Kui kilp ei ole kasutusel või kui kilbi punkte on 0 või vähem, siis vähenevad elu punktid
            // If the shield is not used or shield points are 0 or less, then health points are decreased
            else
            {
                currentHealth -= 5;
                healthPoints.text = "ELU PUNKTID: " + currentHealth.ToString();
            }
        }
    }

    // Mängija füüsilised mängu objektid hävitatakse ja määratakse mängija elus olemise tõeväärtuse valeks
    // Player physical game objects are destroyed
    private void CharacterDestroy()
    {
        spriteRenderer.sprite = deathSprite;
        Destroy(playerRigidbody2D);
        Destroy(playerBoxCollider2D);
        spriteRenderer.sortingLayerName = "Dead";
        playerIsAlive = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damageTimer <= 0f)
        {
            ShieldCheck(collision);

            // Kui mängija elu punktid on otsas
            // If player health points are depleted
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ShieldCheck(collision);

        // Kui mängija elu punktid on otsas
        // If player health points are depleted
        if (currentHealth <= 0)
        {
            CharacterDestroy();
        }
    }

    // Mängitav karakter vaataks hiire kursori poole
    // Makes the player character face wherever the mouse cursor is
    private void LookAtMouse()
    {
        if (playerIsAlive)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.right = mousePos - new Vector2(transform.position.x, transform.position.y);
        }
    }

    // Annab mängijale võimaluse lüüa kurikaga
    // Gives the player the ability to melee attack enemies
    private void Melee()
    {
        if (playerIsAlive)
        {
            if (Input.GetMouseButtonDown(0) && meleeTimer <= 0f)
            {
                // Kuvab ründamise animatsiooni
                // Displays the attack animation
                anim.SetTrigger("Attack");
                meleeTimer = meleeSpeed;
            }
            else
            {
                // Vähendab taimeri väärtusi
                // Decreases the timer variables
                meleeTimer -= Time.deltaTime;
            }
        } 
    }

    // Saab mängitavat karakterit liigutada 'WASD' nuppudega või noolte nuppudega
    // Makes the player character move with the 'WASD' or arrow inputs
    private void Movement()
    {
        if (playerIsAlive) 
        {
            // Kontrollib, kas mängija liigub 
            // Checks if the player character is moving
            if (playerRigidbody2D.velocity.x != 0 || playerRigidbody2D.velocity.y != 0)
            {
                // Kontrollib spriteTimer'it, et sprite'id ei vahetuks liiga kiiresti
                // Checks the spriteTimer so that the sprites would not change too often
                // Kontrollib shootingSpriteTimer'it, et tulistamise sprite'i näidatakse natuke kauem
                // Checks the shootingSpriteTimer so that the shooting sprite would show for a little longer
                if (spriteTimer <= 0f && shootingSpriteTimer <= 0f)
                {
                    // Algväärtustab sprite'ide massiivi 0-i, kui on jõutud viimase elemendini
                    // Resets the sprite array back to the first element if spriteCount has reached the end of the array
                    if (spriteCount > (spriteArray.Length - 1))
                    {
                        spriteCount = 0;
                    }

                    spriteRenderer.sprite = spriteArray[spriteCount];
                    spriteCount++;
                    spriteTimer = spriteRate;
                }
                else
                {
                    // Vähenda taimeri väärtust
                    // Decrease the timer variable
                    spriteTimer -= Time.deltaTime;
                }
            }
            else
            {
                // Kui karakter enam ei tulista, siis näita liikumis sprite'i
                // If character is no longer shooting then show the base not moving sprite
                if (shootingSpriteTimer <= 0f)
                {
                    spriteRenderer.sprite = spriteArray[0];
                }
            }

            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");

            moveInput.Normalize();

            playerRigidbody2D.velocity = moveInput * moveSpeed;
        }
    }

    // Annab mängijale võimaluse tulistada püstoliga ja muudab karakteri sprite tulistamis sprite'iks
    // Gives the player character ability to shoot and changes the character sprite to a shooting sprite
    private void Shooting()
    {
        if (playerIsAlive) 
        {
            // Tulistamiseks peab kasutaja vajutama vasakut hiire klõpsu
            // For shooting, the player has to press the left mouse button
            if (Input.GetMouseButtonDown(0) && fireRateTimer <= 0f)
            {
                // Ilmutab kuuli ja määrab tulistamise kiiruse
                // Spawns the bullet and sets fire rate
                Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation);
                fireRateTimer = fireRate;

                spriteRenderer.sprite = shootingSprite;
                shootingSpriteTimer = shootingSpriteRate;
            }
            else
            {
                // Vähendab taimeri väärtusi
                // Decreases the timer variables
                if (!(shootingSpriteTimer <= 0f))
                {
                    shootingSpriteTimer -= Time.deltaTime;
                }
                fireRateTimer -= Time.deltaTime;
            }
        }
    }
}
