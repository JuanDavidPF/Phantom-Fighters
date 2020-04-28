using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlayerScript : MonoBehaviour {

    /////////////////////////Global determinations///////////////

    [HideInInspector]
    public bool isGamePaused;
    public bool isOutOfBounds;

    /////////////////////////Components///////////////
    private Rigidbody2D physics;
    private Animator animate;
    private CapsuleCollider2D hitbox;

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

    [Tooltip ("Allows player to hold down 'Crouch' button, to get down of a platform")]
    public bool canGetDown;
    [Tooltip ("How much seconds the player has to hold the button to trasspass the platform")]
    public float getDownChargeMax;
    private float getDownCharge;

    private GameObject PlatformUnder;

    /////////////////////////////Controls////////////////////////////////
    private string jumpBtn;
    private string WalkBtn;
    private string crouchBtn;

    /////////////////////////////Health////////////////////////////////
    [Space (5)]
    [Header ("Healt")]
    [Tooltip ("How many times the player is able to die and respawn")]
    public int lives;
    [Tooltip ("What 's the maximum health of the player")]
    [Range (2000, 4000)]
    public int maxHealth = 2000;
    private int health;
    [Tooltip ("Links the health level to an interface healthbar ")]
    public HealthBar healthBar;

    [Tooltip ("How many seconds does the player has to wait to respawn")]
    [Range (1, 10)]
    public int respawnCountdown;
    private bool itLives;
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void Start () {

        //link components
        physics = GetComponent<Rigidbody2D> ();
        hitbox = GetComponent<CapsuleCollider2D> ();
        animate = GetComponentInChildren<Animator> ();

        //Control mapping determined by which player is in control 
        switch (playerIndex) {
            case Players.player1:

                jumpBtn = "Player1Jump";
                WalkBtn = "Player1Horizontal";
                crouchBtn = "Player1Crouch";

                break;

            case Players.player2:
                jumpBtn = "Player2Jump";
                WalkBtn = "Player2Horizontal";
                crouchBtn = "Player2Crouch";
                break;
        }

        //converts seconds to frames
        jumpChargeTimer = jumpChargeTimer * 60;
        getDownChargeMax = getDownChargeMax * 60;

        //fill the healthbar
        itLives = true;
        health = maxHealth;
        if (healthBar != null) healthBar.SetMaxHealth (maxHealth);

    } //closes start method

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void FixedUpdate () {

        //////////////////////////////////Move Conditions///////////////////////////////////

        Move ();

        //////////////////////////////////Ground Check///////////////////////////////////

        StartCoroutine (Grounded ());

    } //closes fixedUpdate method

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void Update () {

        if (!isGamePaused && itLives) {

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

                //////////////////////////////////Get Down Conditions///////////////////////////////////
                //if the player can get down of platforms 
                if (canGetDown) {

                    if (Input.GetButtonDown (crouchBtn)) {
                        getDownCharge = 0;
                        StopCoroutine (GetDown (transform.parent.gameObject));
                        PlatformUnder = null;
                    }

                    if (Input.GetButton (crouchBtn) && PlatformUnder == null && getDownCharge < getDownChargeMax) {
                        getDownCharge++;

                        //get down of the platform if the charge has been reached and the platform is just 1 unit of height

                        if (getDownCharge >= getDownChargeMax && transform.parent.gameObject.transform.localScale.y <= 1) {
                            StartCoroutine (GetDown (transform.parent.gameObject));
                        }

                    }
                    if (Input.GetButtonUp (crouchBtn)) {
                        getDownCharge = 0;

                    }
                }
            } //closes the grounded condition

            //the player fell down or escaped the gameSpace and its life will drain on a second
            if (isOutOfBounds) {
                takeDamage ((int) Mathf.Round (maxHealth / 1f * Time.deltaTime));

            }

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
    IEnumerator GetDown (GameObject platform) {

        hitbox.isTrigger = true;
        PlatformUnder = platform;
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

    IEnumerator Grounded () {

        //Detect if the player is on the ground
        isGrounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, whatIsGround);
        animate.SetBool ("grounded", isGrounded);
        yield return null;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    void OnTriggerEnter2D (Collider2D collider) {

        //groups the player to the platform he is on
        if (collider.gameObject.CompareTag ("Platforms")) {
            transform.parent = collider.transform;
        }
    } //closes method OnTrigger enter

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void OnTriggerExit2D (Collider2D collider) {

        //Ungroups the player from the platform
        if (collider.gameObject.CompareTag ("Platforms")) {
            transform.parent = null;
        }

        //detects if the player got down of a platform

        if (collider.gameObject == PlatformUnder) {
            hitbox.isTrigger = false;
            PlatformUnder = null;
        }

    } //closes method OnTrigger exit
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void takeDamage (int damage) {

        if (itLives && !isGamePaused) {
            health -= damage;
            if (health <= 0) {
                isOutOfBounds = false;
                Die ();
            }

            if (healthBar != null) healthBar.SetHealth (health);
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////d//////

    void Die () {
        itLives = false;
        lives -= 1;

        if (lives > 0) {
            StartCoroutine (Spawn (new Vector3 (0, 0, 0)));
        }

    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    IEnumerator Spawn (Vector3 spawnPosition) {

        while (health < maxHealth) {
            yield return new WaitForSeconds (1 / 60);
            health += (int) Mathf.Round (maxHealth / respawnCountdown * Time.deltaTime);
            healthBar.SetHealth (health);

        }

        health = maxHealth;
        itLives = true;
        transform.position = spawnPosition;
        physics.velocity = new Vector3 (0, 0, 0);
        yield return null;
    }
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    void OnDrawGizmosSelected () {

        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere (groundCheck.position, groundCheckRadius);

    }
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    //These method transforms a value from a scale to other scale proportionally. Ex: 5 in scaleA (1,10) => becomes =>  50 in scaleB (1,100)  
    float ChangeNumberScale (float valueToMap, float initialScaleMin, float initialScaleMax, float finalScaleMin, float finalScaleMax) {
        float MappedValue = finalScaleMin + (valueToMap - initialScaleMin) * (finalScaleMax - finalScaleMin) / (initialScaleMax - initialScaleMin);
        return MappedValue;
    } //closes method ChangeNumberscale
} //closes player movement class