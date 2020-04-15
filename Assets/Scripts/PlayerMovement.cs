using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    //components
    private Rigidbody2D physics;
    private Animator animate;

    //movement
    [Range (1, 100)]
    public float moveSpeed = 10f;
    [Range (1, 20)]
    public float jumpForce = 20f;
    private float moveDirection;
    public bool isFacingRight = true;

    //grounded
    public Transform groundCheck;
    private bool isGrounded;
    private float groundCheckRadius = 0.2f;
    public LayerMask whatIsGround;

    // Start is called before the first frame update
    void Start () {

        physics = GetComponent<Rigidbody2D> ();
        animate = GetComponentInChildren<Animator> ();

    } //closes start method

    // Update is called once per frame
    void FixedUpdate () {

        //move horizontally de player
        moveDirection = Input.GetAxis ("Horizontal");
        animate.SetFloat ("run", Mathf.Abs (moveDirection));
        physics.velocity = new Vector2 (moveDirection * moveSpeed, physics.velocity.y);

        //check if the player is on the ground
        isGrounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, whatIsGround);
        animate.SetBool ("grounded", isGrounded);

        //check if the player is in midair rising or falling

        animate.SetFloat ("verticalDirection", physics.velocity.y);

    } //closes fixedUpdate method

    void Update () {

        //jump
        if (isGrounded && Input.GetKeyDown ("w")) {
            physics.AddForce (new Vector2 (0, jumpForce * 150));
        }

        //flip the sprite in the direction is facing
        if ((moveDirection > 0 && !isFacingRight) || (moveDirection < 0 && isFacingRight)) {
            FlipSprite ();
        }
    }

    void FlipSprite () {
        isFacingRight = !isFacingRight;
        float VerticalFlip = transform.localScale.x * -1;
        transform.localScale = new Vector3 (VerticalFlip, transform.localScale.y, transform.localScale.z);

    }
} //closes player movement class