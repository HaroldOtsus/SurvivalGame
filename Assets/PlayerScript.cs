using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    public TextMeshProUGUI healthPoints;
    public TextMeshProUGUI shieldPoints;
    public Sprite[] weaponSprites;
    public Image equippedWeaponImage;
    private int equippedWeaponIndex = 1;
    public GameObject gunObject;
    public Sprite shieldSprite;

    // Moving variables
    public Rigidbody2D playerRigidbody2D;
    public BoxCollider2D playerBoxCollider2D;
    public float moveSpeed;
    private Vector2 moveInput;

    // Logic Variables
    public LogicManagerScript logicManager;
    public bool playerIsAlive = true;

    // Shooting variables
    public GameObject bulletPrefab;
    public Transform firingPoint;
    public float fireRate = 2f;                 // Determines how fast the bullets will shoot out
    public float fireRateTimer;
    public float shootingSpriteRate = 0.25f;    // Determines how long the shooting sprite will stay as the displayed sprite
    private float shootingSpriteTimer;          

    // Sprite variables
    public Sprite[] spriteArray;
    public Sprite shootingSprite;
    public Sprite deathSprite;
    public SpriteRenderer spriteRenderer;
    private int spriteCount = 0;                // Initialized to '0' to show the first sprite in the spriteArray
    public float spriteRate = 0.08f;            // Determines how fast the sprites will change when the character is in move
    private float spriteTimer;

    // Health variables
    public int maxHealth = 100;
    public int currentHealth;
    public int shieldHealth;

    // Melee variables
    public GameObject meleeWeapon;
    public Animator anim;
    public float meleeSpeed;
    public float meleeTimer;

    public float damageSpeed;
    public float damageTimer;

    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "GameScene")
        {
            currentHealth = maxHealth;
            shieldHealth = maxHealth;
            healthPoints.text = "HP: " + currentHealth.ToString();
            shieldPoints.text = "SP: " + shieldHealth.ToString();
            logicManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManagerScript>();
        }
        equippedWeaponImage.sprite = weaponSprites[0];
    }

    void Update()
    {
        if (!playerIsAlive)
        {
            SceneManager.LoadScene("GameOverScene");
        }
        LookAtMouse();
        Movement();
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            shootingSpriteTimer = 0;
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
            meleeWeapon.SetActive(false);
            gunSprite.sprite = null;
            Shooting();
        }
        else if (equippedWeaponIndex == 2)
        {
            meleeWeapon.SetActive(true);
            Melee();
        }
        else if (equippedWeaponIndex == 3 && shieldHealth > 0)
        {
            meleeWeapon.SetActive(false);
            gunSprite.sprite = shieldSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damageTimer <= 0f)
        {
            if (collision.gameObject.tag == "Melee")
            {
                if (equippedWeaponIndex == 3 && shieldHealth > 0)
                {
                    shieldHealth -= 5;
                    shieldPoints.text = "SP: " + shieldHealth.ToString();
                }
                else
                {
                    currentHealth -= 5;
                    healthPoints.text = "HP: " + currentHealth.ToString();
                }
            }
            if (currentHealth <= 0)
            {
                spriteRenderer.sprite = deathSprite;
                Destroy(playerRigidbody2D);
                Destroy(playerBoxCollider2D);
                spriteRenderer.sortingLayerName = "Dead";
                playerIsAlive = false;
            }
            damageTimer = damageSpeed;
        }
        else
        {
            damageTimer -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            if (equippedWeaponIndex == 3 && shieldHealth > 0)
            {
                shieldHealth -= 10;
                shieldPoints.text = "SP: " + shieldHealth.ToString();
            }
            else
            {
                currentHealth -= 10;
                healthPoints.text = "HP: " + currentHealth.ToString();
            }
        }
        if (currentHealth <= 0)
        {
            spriteRenderer.sprite = deathSprite;
            Destroy(playerRigidbody2D);
            Destroy(playerBoxCollider2D);
            spriteRenderer.sortingLayerName = "Dead";
            playerIsAlive = false; 
        }
    }

    // Makes the player character face wherever the mouse is
    private void LookAtMouse()
    {
        if (playerIsAlive)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.right = mousePos - new Vector2(transform.position.x, transform.position.y);
        }
    }

    private void Melee()
    {
        if (playerIsAlive)
        {
            if (Input.GetMouseButtonDown(0) && meleeTimer <= 0f)
            {
                anim.SetTrigger("Attack");
                meleeTimer = meleeSpeed;
            }
            else
            {
                meleeTimer -= Time.deltaTime;
            }
        } 
    }

    // Makes the player character move with the 'WASD' or arrow inputs
    private void Movement()
    {
        if (playerIsAlive) 
        {
            // Checks if the player character is moving
            if (playerRigidbody2D.velocity.x != 0 || playerRigidbody2D.velocity.y != 0)
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

            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");

            // Vector keeps the same direction, but the length is 1.0
            moveInput.Normalize();

            playerRigidbody2D.velocity = moveInput * moveSpeed;
        }
    }

    // Makes the player character shoot and changes the character sprite to a shooting sprite when mouse1 button is pressed
    private void Shooting()
    {
        if (playerIsAlive) 
        {
            if (Input.GetMouseButtonDown(0) && fireRateTimer <= 0f)
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
    }
}
