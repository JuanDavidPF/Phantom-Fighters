using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlayerMovement : MonoBehaviour {

    /////////////////////////Global determinations///////////////

    [HideInInspector]
    public bool isGamePaused;

    /////////////////////////Components///////////////
    private Rigidbody2D physics;
    private Animator animate;

    /////////////////////////Variables with predifined values///////////////

    public enum Players { player1, player2 }

    public enum Characters { Kiryl, Scafander, Death }

    /////////////////////////Player control////////////////////////////////

    [Space (5)]
    [Header ("Character Identifier")]
    [Tooltip ("Determines what player is controlling the character")]
    public Players playerIndex;

    [Tooltip ("Determines what mechanics your character can perform")]
    public Characters character;

    /////////////////////////Horizontal Movement////////////////////////////////
    [Space (5)]
    [Header ("Movement")]

    [Tooltip ("Determines the speed of horizontal movement")]
    [Range (1, 100)]
    public float moveSpeed;

    [Tooltip ("True if the player is moving right. Check if the sprite is looking to the right, Uncheck if the sprite is looking to the left.")]
    public bool isFacingRight = true;

    private float moveDirection;

    /////////////////////////Vertical Movement////////////////////////////////
    [Space (5)]
    [Header ("Jump")]
    [Tooltip ("Determines how much base force the jump has")]
    [Range (1, 20)]
    public float jumpForce;

    [Tooltip ("Determines how many seconds the jump button needs to be pressed to reach max height jump")]
    [Range (0, 5)]
    public float jumpChargeTimer;
    private float jumpCharge;
    private IEnumerator jumpCoroutine;

    private bool isJumpCoroutineRunning;

    [Tooltip ("Determines the range of the jump boost, the X determines the lowest jump boost possible and the Y the Maximun jump boost posibble")]

    public Vector2 jumpBoostInterval;

    /////////////////////////Ground Detection////////////////////////////////
    [Space (5)]
    [Header ("Ground Detection")]
    [Tooltip ("Trigger that detects contact with the ground")]
    public Transform groundCheck;

    [Tooltip ("Trigger that determines the size of the ground trigger")]
    [Range (0.1f, 0.5f)]
    public float groundCheckRadius = 0.2f;

    [Tooltip ("Determines what layers the trigger will count as a ground")]
    public LayerMask whatIsGround;
    private bool isGrounded;

    /////////////////////////////Controls////////////////////////////////
    private string jumpBtn;
    private string WalkBtn;

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void Start () {

        //link components
        physics = GetComponent<Rigidbody2D> ();
        animate = GetComponentInChildren<Animator> ();

        //Control mapping determined by which player is in control 
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

        //converts seconds to frames
        jumpChargeTimer = jumpChargeTimer * 60;
    } //closes start method

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void FixedUpdate () {

        //////////////////////////////////Move Conditions///////////////////////////////////

        Move ();

        //////////////////////////////////Ground Check///////////////////////////////////

        Grounded ();

    } //closes fixedUpdate method

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void Update () {

        if (!isGamePaused) {

            //flip the sprite in the direction the player is facing
            if ((moveDirection > 0 && !isFacingRight) || (moveDirection < 0 && isFacingRight)) {
                FlipSprite ();
            }

            //animate if the player on the air is Rising or Falling
            if (!isGrounded) animate.SetFloat ("verticalDirection", physics.velocity.y);

            if (isGrounded) {

                //////////////////////////////////Jump Conditions///////////////////////////////////

                //resets the jumping state everytime the jumpbtn is pressed
                if (Input.GetButtonDown (jumpBtn)) {

                    if (jumpCoroutine != null) {
                        StopCoroutine (jumpCoroutine);
                        jumpCoroutine = null;
                    }
                    jumpCharge = 0;

                }

                //Jump height increases if  the player holds the jump button
                if (Input.GetButton (jumpBtn) && jumpCoroutine == null) {
                    jumpCharge++;

                    if (jumpCharge >= jumpChargeTimer) {
                        jumpCoroutine = Jump ();
                        StartCoroutine (jumpCoroutine);

                    }
                }
                //Jump is less strong if the player taps and releases the jump button
                else if (Input.GetButtonUp (jumpBtn) && jumpCharge < jumpChargeTimer && jumpCoroutine == null) {

                    //dimishes in a 35% the jump boost charged before the button was released
                    jumpCharge = jumpCharge - jumpCharge * 0.35f;
                    jumpCoroutine = Jump ();
                    StartCoroutine (jumpCoroutine);

                }
            } //closes the grounded condition

        } //closes the pause condition

    } //closes the update method

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void FlipSprite () {
        isFacingRight = !isFacingRight;
        float VerticalFlip = transform.localScale.x * -1;
        transform.localScale = new Vector3 (VerticalFlip, transform.localScale.y, transform.localScale.z);
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    IEnumerator Jump () {
        jumpCharge = ChangeNumberScale (jumpCharge, 0, jumpChargeTimer, jumpBoostInterval.x, jumpBoostInterval.y);
        physics.AddForce (new Vector2 (0, jumpForce * jumpCharge));
        jumpCharge = 0;
        yield return null;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    void Move () {

        //move the player horizontally
        moveDirection = Input.GetAxis (WalkBtn);
        animate.SetFloat ("run", Mathf.Abs (moveDirection));
        physics.velocity = new Vector2 (moveDirection * moveSpeed, physics.velocity.y);
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void Grounded () {
        //Detect if the player is on the ground
        isGrounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, whatIsGround);
        animate.SetBool ("grounded", isGrounded);
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    //These method transforms a value from a scale to other scale proportionally. Ex: 5 in scaleA (1,10) => becomes =>  50 in scaleB (1,100)  
    float ChangeNumberScale (float valueToMap, float initialScaleMin, float initialScaleMax, float finalScaleMin, float finalScaleMax) {
        float MappedValue = finalScaleMin + (valueToMap - initialScaleMin) * (finalScaleMax - finalScaleMin) / (initialScaleMax - initialScaleMin);
        return MappedValue;
    }
} //closes player movement class