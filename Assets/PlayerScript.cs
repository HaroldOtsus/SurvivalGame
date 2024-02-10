using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody2D playerRigidbody2D;
    public float moveSpeed;
    private Vector2 moveInput;

    public LogicManagerScript logicManager;
    public bool playerIsAlive = true;
    

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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        logicManager.gameOver();
        playerIsAlive = false;
    }

    // Makes the player character face wherever the mouse is
    private void LookAtMouse()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.right = mousePos - new Vector2(transform.position.x, transform.position.y);
    }

    // Makes the player character move with the 'WASD' or arrow inputs
    private void Movement()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Vector keeps the same direction, but the length is 1.0
        moveInput.Normalize();

        playerRigidbody2D.velocity = moveInput * moveSpeed;
    }
}
