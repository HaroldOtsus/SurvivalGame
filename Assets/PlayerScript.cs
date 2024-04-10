using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
    // Kasutajaliidese muutujad - UI variables
    public TextMeshProUGUI healthPoints;        // Elu punktide n�itamiseks
    public TextMeshProUGUI shieldPoints;        // Kilbi punktide n�itamiseks
    public Image equippedWeaponImage;           // Kasutuses oleva relva pilt
    public Sprite[] weaponSprites;              // Relvade piltide massiiv
    public Sprite shieldSprite;                 // Kilbi pilt

    // Hetkel kasutuses oleva relva n�itamiseks - For displaying current weapon
    private int equippedWeaponIndex = 1;
    public GameObject gunObject;                // Relva objekt

    // Liikumise muutujad - Moving variables
    public Rigidbody2D playerRigidbody2D;       // F��sikaline komponent - Physical component
    public BoxCollider2D playerBoxCollider2D;   // Kokkup�rgete tuvastamiseks - To detect collisions
    public float moveSpeed;                     // M�ngitava karakteri kiirus - Playable character movement speed
    private Vector2 moveInput;                  // Kasutaja sisestus karakteri liikutamiseks - User input to move character

    // Loogika muutujad - Logic Variables
    public LogicManagerScript logicManager;
    public bool playerIsAlive = true;           // T�si, kui m�ngitav karakter on elus

    // Tulistamise muutujad - Shooting variables
    public GameObject bulletPrefab;             // Kuuli prefab m�nguobjekt
    public Transform firingPoint;               // Asukoht, kust kuul v�lja lendab - Point where the bullet spawns from
    public float fireRate = 2f;                 // Tulistamis kiirus - Determines how fast the bullets will shoot out
    public float fireRateTimer;

    // Tulistamise sprite'i n�itamise aeg sekundites - Determines how long the shooting sprite will stay as the displayed sprite(in seconds)
    public float shootingSpriteRate = 0.25f;    
    private float shootingSpriteTimer;          
    
    // M�ngitava karakteri visuaalsete spraitide muutujad - Character sprite variables
    public Sprite[] spriteArray;                // Massiiv karakteri sprite'idest
    public Sprite shootingSprite;               // Tulistamist kujutav sprite
    public Sprite deathSprite;                  // L��a saanud m�ngitava karakteri sprite
    public SpriteRenderer spriteRenderer;       // Kuvab sprite
    private int spriteCount = 0;                // 0, et n�idata esimest sprite'i massiivis - Initialized to '0' to show the first sprite in the spriteArray

    // M��rab, kui kiiresti sprite'id vahetuvad liikuval karakteril - Determines how fast the sprites will change when the character is in move
    public float spriteRate = 0.08f;            
    private float spriteTimer;

    // Elude muutujad - Health variables
    public int maxHealth;                   // Maksimum elu punktid - Maximum health points
    public int currentHealth;               // Hetke elu punktid
    public int shieldHealth;                // Kilbi punktid

    // L�hiv�itluse muutujad - Melee variables
    public GameObject meleeWeapon;          // L�hiv�itluse relva m�ngu objekt
    public Animator anim;                   // Animatsioonide jaoks
    public float meleeSpeed;                // L�hiv�itluse l��gi kiirus
    public float meleeTimer;

    // Vigastuse muutujad - Damage variables
    // Vigastuse saamise kiirus, vajalik, et m�ngija ei saaks liiga palju vigastusi liiga kiiresti kurikate k�est
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
        // M�ng l�bi, kui m�ngija h�vitatakse
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

            // K�ib l�bi j�rjest k�ik elemendid kuni viimase elemendini
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
            // Toob kurika n�htavale
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

        // Laadi m�ng l�bi stseen
        // Loads the game over scene
        SceneManager.LoadScene("GameOverScene");
    }

    private void ShieldCheck(Collider2D collision)
    {
        // Kui kasutaja saab pihta vastase kurikaga v�i kuuliga
        // If the player gets hit with enemy melee attacks or bullets
        if (collision.gameObject.tag == "EnemyMelee")
        {
            // Kui kilp on kasutusel, siis kilbi punktid v�henevad
            // If the shield is in use, then the shield points decrease
            if (equippedWeaponIndex == 3 && shieldHealth > 0)
            {
                shieldHealth -= 5;
                shieldPoints.text = "KILBI PUNKTID: " + shieldHealth.ToString();

                // Kui kilp on kasutusel, siis m�ngija saab elu punkte r�nnakute eest
                // If the shield is used, then the player gets health points back against attacks
                currentHealth += 5;
                healthPoints.text = "ELU PUNKTID: " + currentHealth.ToString();
            }
            // Kui kilp ei ole kasutusel v�i kui kilbi punkte on 0 v�i v�hem, siis v�henevad elu punktid
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
        // Kui kasutaja saab pihta vastase kurikaga v�i kuuliga
        // If the player gets hit with enemy melee attacks or bullets
        if (collision.gameObject.tag == "EnemyBullet")
        {
            // Kui kilp on kasutusel, siis kilbi punktid v�henevad
            // If the shield is in use, then the shield points decrease
            if (equippedWeaponIndex == 3 && shieldHealth > 0)
            {
                shieldHealth -= 5;
                shieldPoints.text = "KILBI PUNKTID: " + shieldHealth.ToString();

                // Kui kilp on kasutusel, siis m�ngija saab elu punkte r�nnakute eest
                // If the shield is used, then the player gets health points back against attacks
                currentHealth += 5;
                healthPoints.text = "ELU PUNKTID: " + currentHealth.ToString();
            }
            // Kui kilp ei ole kasutusel v�i kui kilbi punkte on 0 v�i v�hem, siis v�henevad elu punktid
            // If the shield is not used or shield points are 0 or less, then health points are decreased
            else
            {
                currentHealth -= 5;
                healthPoints.text = "ELU PUNKTID: " + currentHealth.ToString();
            }
        }
    }

    // M�ngija f��silised m�ngu objektid h�vitatakse ja m��ratakse m�ngija elus olemise t�ev��rtuse valeks
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

            // Kui m�ngija elu punktid on otsas
            // If player health points are depleted
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ShieldCheck(collision);

        // Kui m�ngija elu punktid on otsas
        // If player health points are depleted
        if (currentHealth <= 0)
        {
            CharacterDestroy();
        }
    }

    // M�ngitav karakter vaataks hiire kursori poole
    // Makes the player character face wherever the mouse cursor is
    private void LookAtMouse()
    {
        if (playerIsAlive)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.right = mousePos - new Vector2(transform.position.x, transform.position.y);
        }
    }

    // Annab m�ngijale v�imaluse l��a kurikaga
    // Gives the player the ability to melee attack enemies
    private void Melee()
    {
        if (playerIsAlive)
        {
            if (Input.GetMouseButtonDown(0) && meleeTimer <= 0f)
            {
                // Kuvab r�ndamise animatsiooni
                // Displays the attack animation
                anim.SetTrigger("Attack");
                meleeTimer = meleeSpeed;
            }
            else
            {
                // V�hendab taimeri v��rtusi
                // Decreases the timer variables
                meleeTimer -= Time.deltaTime;
            }
        } 
    }

    // Saab m�ngitavat karakterit liigutada 'WASD' nuppudega v�i noolte nuppudega
    // Makes the player character move with the 'WASD' or arrow inputs
    private void Movement()
    {
        if (playerIsAlive) 
        {
            // Kontrollib, kas m�ngija liigub 
            // Checks if the player character is moving
            if (playerRigidbody2D.velocity.x != 0 || playerRigidbody2D.velocity.y != 0)
            {
                // Kontrollib spriteTimer'it, et sprite'id ei vahetuks liiga kiiresti
                // Checks the spriteTimer so that the sprites would not change too often
                // Kontrollib shootingSpriteTimer'it, et tulistamise sprite'i n�idatakse natuke kauem
                // Checks the shootingSpriteTimer so that the shooting sprite would show for a little longer
                if (spriteTimer <= 0f && shootingSpriteTimer <= 0f)
                {
                    // Algv��rtustab sprite'ide massiivi 0-i, kui on j�utud viimase elemendini
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
                    // V�henda taimeri v��rtust
                    // Decrease the timer variable
                    spriteTimer -= Time.deltaTime;
                }
            }
            else
            {
                // Kui karakter enam ei tulista, siis n�ita liikumis sprite'i
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

    // Annab m�ngijale v�imaluse tulistada p�stoliga ja muudab karakteri sprite tulistamis sprite'iks
    // Gives the player character ability to shoot and changes the character sprite to a shooting sprite
    private void Shooting()
    {
        if (playerIsAlive) 
        {
            // Tulistamiseks peab kasutaja vajutama vasakut hiire kl�psu
            // For shooting, the player has to press the left mouse button
            if (Input.GetMouseButtonDown(0) && fireRateTimer <= 0f)
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
    }
}
