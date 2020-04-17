using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlayerMovement : MonoBehaviour {

    //components
    private Rigidbody2D physics;
    private Animator animate;

    public enum Players { player1, player2 }
    public enum Characters { Kiryl, Scafander, Death }

    [Header ("Character Identifier")]

    public Players playerIndex;
    public Characters character;

    [Header ("Movement")]
    [Range (1, 100)]
    public float moveSpeed;
    [Range (1, 20)]
    public float jumpForce;
    private float jumpCharge;
    public float jumpChargeMax;
    public bool isJumpCoroutineRunning;

    private float moveDirection;
    public bool isFacingRight = true;

    [Header ("Grounded")]
    public Transform groundCheck;
    private bool isGrounded;

    [Range (0.1f, 0.5f)]
    public float groundCheckRadius = 0.2f;
    public LayerMask whatIsGround;

    [HideInInspector]
    public bool isGamePaused;

    private string jumpBtn;
    private string WalkBtn;
    // Start is called before the first frame update
    void Start () {

        physics = GetComponent<Rigidbody2D> ();
        animate = GetComponentInChildren<Animator> ();

        //mapeo de los controles
        switch (playerIndex) {
            case Players.player1:

                jumpBtn = "Player1Jump";
                WalkBtn = "Player1Horizontal";
                break;

            case Players.player2:
                jumpBtn = "Player2Jump";
                WalkBtn = "Player2Horizontal";
                break;

        }

    } //closes start method

    // Update is called once per frame
    void FixedUpdate () {

        //move the player horizontally

        moveDirection = Input.GetAxis (WalkBtn);
        animate.SetFloat ("run", Mathf.Abs (moveDirection));
        physics.velocity = new Vector2 (moveDirection * moveSpeed, physics.velocity.y);

        //check if the player is on the ground
        isGrounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, whatIsGround);
        animate.SetBool ("grounded", isGrounded);

        //check if the player is in midair rising or falling
        animate.SetFloat ("verticalDirection", physics.velocity.y);

    } //closes fixedUpdate method

    void Update () {

        if (!isGamePaused) {

            if (isGrounded) {

                if (Input.GetButtonDown (jumpBtn)) {
                    jumpCharge = 0;
                    isJumpCoroutineRunning = false;
                }

                //hold and jump
                if (Input.GetButton (jumpBtn) && isJumpCoroutineRunning == false) {
                    jumpCharge += 0.5f;
                    if (jumpCharge >= jumpChargeMax) {
                        isJumpCoroutineRunning = true;
                        StopCoroutine ("Jump");
                        StartCoroutine (Jump ());
                    }
                }

                //Tap and Jump
                if (Input.GetButtonUp (jumpBtn) && jumpCharge < jumpChargeMax && isJumpCoroutineRunning == false) {
                    isJumpCoroutineRunning = true;
                    StopCoroutine ("jump");
                    StartCoroutine (Jump ());

                }
            }
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

    IEnumerator Jump () {

        jumpCharge = Map (jumpCharge, 0, jumpChargeMax, 120, 170);
        physics.AddForce (new Vector2 (0, jumpForce * jumpCharge));
        jumpCharge = 0;
        yield return null;
    }

    float Map (float value, float fromMin, float fromMax, float toMin, float toMax) {
        return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);

    }
} //closes player movement class