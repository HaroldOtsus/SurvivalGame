using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // Moving variables
    public Rigidbody2D playerRigidbody2D;
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
    public SpriteRenderer spriteRenderer;
    private int spriteCount = 0;                // Initialized to '0' to show the first sprite in the spriteArray
    public float spriteRate = 0.08f;            // Determines how fast the sprites will change when the character is in move
    private float spriteTimer;


    // Start is called before the first frame update
    void Start()
    {
        logicManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManagerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        LookAtMouse();
        Movement();
        Shooting();
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        logicManager.gameOver();
        playerIsAlive = false;
    }*/

    // Makes the player character face wherever the mouse is
    private void LookAtMouse()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.right = mousePos - new Vector2(transform.position.x, transform.position.y);
    }

    // Makes the player character move with the 'WASD' or arrow inputs
    private void Movement()
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

    // Makes the player character shoot and changes the character sprite to a shooting sprite when mouse1 button is pressed
    private void Shooting()
    {
        if (Input.GetMouseButtonDown(0) && fireRateTimer <= 0f)
        {
            // Spawns the bullet and sets fire rate
            Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation);
            fireRateTimer = fireRate;

            // 
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
