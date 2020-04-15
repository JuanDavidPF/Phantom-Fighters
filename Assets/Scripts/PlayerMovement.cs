using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    //components
    private Rigidbody2D physics;

    //movement
    [Range (1, 100)]
    public float moveSpeed = 10f;
    [Range (1, 20)]
    public float jumpForce = 20f;
    private float moveDirection;

    public bool isFacingRight = true;

    // Start is called before the first frame update
    void Start () {

        physics = GetComponent<Rigidbody2D> ();

    } //closes start method

    // Update is called once per frame
    void FixedUpdate () {

        moveDirection = Input.GetAxis ("Horizontal");
        physics.velocity = new Vector2 (moveDirection * moveSpeed, physics.velocity.y);

    } //closes fixedUpdate method

    void Update () {

        if (Input.GetKeyDown ("w")) {
            physics.AddForce (new Vector2 (0, jumpForce * 150));
        }
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