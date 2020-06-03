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
    private Collider2D hitbox;
    SpriteRenderer graphics;
    private Collider2D enemyHitbox;

    /////////////////////////Variables with predifined values///////////////

    public enum Players { none, player1, player2 }

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

    private bool isMoving;

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
    private string lightAttackBtn;
    private string midAttackBtn;

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

    [HideInInspector]
    public bool itLives;

    [HideInInspector]
    private bool isKnockedBack;

    /////////////////////////////Attack////////////////////////////////
    [Space (5)]
    [Header ("Attack")]
    [Tooltip ("Object that gives the position of the center of the damage zone")]
    public Transform attackPoint;
    [Tooltip ("Size of the hitbox that deals damage to the oponents")]
    public float attackRange;
    [Tooltip ("What can be hurted")]
    public LayerMask whatIsEnemy;

    [Header ("Attack Damage")]
    [Space (5)]

    [Tooltip ("What is the base damage of the player's light attacks")]
    [Range (100, 1800)]
    public int lightAttackDamage;

    [Tooltip ("What is the base damage of the player's mid attacks")]
    [Range (100, 1800)]
    public int midAttackDamage;

    [Tooltip ("What is the base damage of the player's heavy attacks")]
    [Range (100, 1800)]
    public int heavyAttackDamage;

    private int attackDamage;

    [Header ("Attack Speed")]
    [Space (5)]

    [Tooltip ("How many times per second the player can light attack")]
    [Range (0.1f, 10)]
    public float lightAttackRate;

    [Tooltip ("How many times per second the player can mid attack")]
    [Range (0.1f, 10)]
    public float midAttackRate;

    [Tooltip ("How many times per second the player can heavy attack")]
    [Range (0.1f, 10)]
    public float heavyAttackRate;

    private float attackRate;
    private float attackCooldown;

    //FX
    [Space (5)]
    [Header ("FX")]
    [Tooltip ("Object that gives the position of the center of the damage zone")]
    public GameObject hurtedLight;

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void Start () {

        //link components
        physics = GetComponent<Rigidbody2D> ();
        hitbox = GetComponent<CapsuleCollider2D> ();
        animate = GetComponentInChildren<Animator> ();
        graphics = GetComponentInChildren<SpriteRenderer> ();

        //Control mapping determined by which player is in control 
        switch (playerIndex) {
            case Players.player1:

                jumpBtn = "Player1Jump";
                WalkBtn = "Player1Horizontal";
                crouchBtn = "Player1Crouch";
                lightAttackBtn = "Player1LightAttack";
                midAttackBtn = "Player1MidAttack";
                break;

            case Players.player2:
                jumpBtn = "Player2Jump";
                WalkBtn = "Player2Horizontal";
                crouchBtn = "Player2Crouch";
                lightAttackBtn = "Player2LightAttack";
                midAttackBtn = "Player2MidAttack";
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

        if (!isGamePaused && itLives) Move ();

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

                    //animate the player taking impulse
                    animate.SetBool ("impulse", true);

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

            //the player attacks
            if (Input.GetButtonDown (lightAttackBtn)) {

                if (Time.time >= attackCooldown) {
                    StartCoroutine (Attack (1));

                    attackCooldown = Time.time + 1f / attackRate;
                }
            } else if (Input.GetButtonDown (midAttackBtn)) {

                if (Time.time >= attackCooldown) {
                    StartCoroutine (Attack (2));

                    attackCooldown = Time.time + 1f / attackRate;
                }
            }

            //the player fell down or escaped the gameSpace and its life will drain on a second
            if (isOutOfBounds) {
                StartCoroutine (TakeDamage ((int) Mathf.Round (maxHealth / 1f * Time.deltaTime)));

            }

        } //closes the pause && it lives condition

        //the dead player fell into the void

        if (!isGamePaused && !itLives && isOutOfBounds) {
            isKnockedBack = false;
            StartCoroutine (Die ());
        }

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
        animate.SetBool ("impulse", false);
        jumpCharge = ChangeNumberScale (jumpCharge, 0, jumpChargeTimer, jumpBoostInterval.x, jumpBoostInterval.y);
        physics.AddForce (new Vector2 (0, jumpForce * jumpCharge));
        jumpCharge = 0;
        yield return null;
    }
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    IEnumerator GetDown (GameObject platform) {
        StartCoroutine (PlayerCollides (platform.GetComponent<Collider2D> (), true));
        PlatformUnder = platform;
        yield return null;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    void Move () {

        //move the player horizontally
        moveDirection = Input.GetAxis (WalkBtn);
        animate.SetFloat ("run", Mathf.Abs (physics.velocity.x));
        if (!isKnockedBack) physics.velocity = new Vector2 (moveDirection * moveSpeed, physics.velocity.y);
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    IEnumerator Grounded () {

        //Detect if the player is on the ground
        isGrounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, whatIsGround);
        animate.SetBool ("grounded", isGrounded);

        //Deactivates the knockedback state when the player touches the ground again
        if (!isGrounded && isKnockedBack) {
            yield return StartCoroutine (Recover ());

        }

        if (isGrounded && !itLives) {

            //the players just stays static when its dead corpse lands
            physics.velocity = Vector2.zero;
        }
        yield return null;
    }
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    IEnumerator Recover () {

        while (!isGrounded) {
            yield return new WaitForSeconds (0.1f);
        }

        animate.SetBool ("hurted", false);
        isKnockedBack = false;
        if (hurtedLight) hurtedLight.SetActive (false);

        yield return null;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    IEnumerator Attack (int attackForce) {

        //set the damage depending of the move atack
        switch (attackForce) {
            case 1:

                attackDamage = lightAttackDamage;
                attackRate = lightAttackRate;
                break;

            case 2:
                attackDamage = midAttackDamage;
                attackRate = midAttackRate;
                break;

            case 3:
                attackDamage = heavyAttackDamage;
                attackRate = heavyAttackRate;
                break;
        }

        //activates the attack animation
        animate.SetTrigger ("attack");
        animate.SetInteger ("attackForce", attackForce);

        //detect anemies in range of attack
        Collider2D[] enemiesInDamageZone = Physics2D.OverlapCircleAll (attackPoint.position, attackRange, whatIsEnemy);

        //deal damage to them
        foreach (Collider2D enemy in enemiesInDamageZone) {

            if (enemy != hitbox) {

                PlayerScript enemyScript = enemy.GetComponent<PlayerScript> ();

                if (enemyScript.itLives) {

                    //deals damage to the enemy
                    StartCoroutine (enemyScript.TakeDamage (attackDamage));

                    //push the enemy away from the player when hitted and makes him face to the player

                    //knockback for damae Amount
                    // float knockbackForce = ChangeNumberScale (attackDamage, 100, enemyScript.maxHealth, 400, 1200);

                    //knockback for health state
                    float knockbackForce = ChangeNumberScale (enemyScript.health, enemyScript.maxHealth, 0, 400, 1200);

                    if (isFacingRight) {
                        knockbackForce = Mathf.Abs (knockbackForce);
                        if (enemyScript.isFacingRight) enemyScript.FlipSprite ();
                        StartCoroutine (enemyScript.Knockback (knockbackForce, Mathf.Abs (knockbackForce) * 1.5f));
                    } else {
                        knockbackForce = Mathf.Abs (knockbackForce) * -1;
                        if (!enemyScript.isFacingRight) enemyScript.FlipSprite ();
                        StartCoroutine (enemyScript.Knockback (knockbackForce, Mathf.Abs (knockbackForce) * 1.5f));
                    }

                    //calculates if the oponent is dead and ignores any colission
                    if (enemyScript.health <= 0) {

                        StartCoroutine (enemyScript.PlayerCollides (hitbox, true));
                    }
                } //condition if the enemmy is alive
            } //condition if the player it's not himself
        }
        yield return null;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    IEnumerator TakeDamage (int damage) {

        if (itLives && !isGamePaused) {

            //deal damage
            health -= damage;

            //get the sprite light red
            if (hurtedLight) hurtedLight.SetActive (true);

            //aimates the hurted state
            animate.SetBool ("hurted", true);

            //updates the healthbar
            if (healthBar != null) healthBar.SetHealth (health);

            //dies
            if (health <= 0) {
                health = 0;
                StartCoroutine (Die ());
            }

        }
        yield return null;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////d//////
    IEnumerator Knockback (float horizontalForceReceived, float verticalForceReceived) {

        isKnockedBack = true;

        switch (character) {

            case Characters.Scafander:
                physics.AddForce (new Vector2 (horizontalForceReceived, verticalForceReceived * 1.5f));
                break;

            case Characters.Kiryl:
                physics.AddForce (new Vector2 (horizontalForceReceived, verticalForceReceived));
                break;

        }

        yield return null;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////d//////

    IEnumerator Die () {

        itLives = false;

        if (isOutOfBounds) {
            isKnockedBack = false;
            isOutOfBounds = false;
        }

        if (!isKnockedBack) {
            animate.SetBool ("die", !itLives);
            lives -= 1;
            graphics.sortingOrder = -1;

            if (lives > 0) {
                StartCoroutine (Spawn (new Vector3 (0, 0, 0)));
            }
        }

        Debug.Log (lives);

        yield return null;
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    IEnumerator Spawn (Vector3 spawnPosition) {

        bool refilling = true;

        while (refilling) {
            yield return new WaitForSeconds (1 / 60);
            health += (int) Mathf.Round (maxHealth / respawnCountdown * Time.deltaTime);
            if (healthBar != null) healthBar.SetHealth (health);

            if (health >= maxHealth) {
                health = maxHealth;
                refilling = false;
            }
        }

        physics.velocity = Vector2.zero;
        graphics.sortingOrder = 1;
        health = maxHealth;
        itLives = true;
        transform.position = spawnPosition;
        animate.SetBool ("die", !itLives);
        animate.SetBool ("hurted", false);
        if (hurtedLight) hurtedLight.SetActive (false);

        //enables to collide with the oponent again
        if (enemyHitbox != null) StartCoroutine (PlayerCollides (enemyHitbox, false));

        yield return null;
    }
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    IEnumerator PlayerCollides (Collider2D enemy, bool collides) {
        enemyHitbox = enemy;
        Physics2D.IgnoreCollision (hitbox, enemy, collides);

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
            StartCoroutine (PlayerCollides (PlatformUnder.GetComponent<Collider2D> (), false));
            PlatformUnder = null;
        }

    } //closes method OnTrigger exit
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void OnDrawGizmosSelected () {

        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere (groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere (attackPoint.position, attackRange);

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