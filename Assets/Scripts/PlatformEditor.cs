using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlatformEditor : MonoBehaviour {

    //////////////////////Dimensions of the rectangular platform in units///////////////

    private int platformLength;
    private int platformHeight;

    /////////////////////////Variables with predifined values///////////////
    public enum Directions { vertical, horizontal, diagonal1, diagonal2 }

    /////////////////////////Apperance///////////////
    [Space (5)]
    [Header ("Appearance")]
    [Tooltip ("Here goes the sprite of the material that hte platform's tiles are going to have")]
    public Sprite material;

    /////////////////////////Movement///////////////

    [Space (5)]
    [Header ("Movement")]
    [Tooltip ("Allows the platfom to move in a particular direction")]
    public bool itMoves;

    [Tooltip ("Sets the direction in wich the platform it's going to move")]
    public Directions direction;

    [Tooltip ("Control of the animation of the platform")]
    public RuntimeAnimatorController movementController;

    private int indexDirection;

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    void Start () {

        //determines the size of the platform, the size is in unity units, so the dimensions need to be positive, rounded integers.
        platformLength = (int) Mathf.Round (Mathf.Abs (transform.localScale.x));
        platformHeight = (int) Mathf.Round (Mathf.Abs (transform.localScale.y));

        //fixes the platform corner position if the object was negative scaled
        if (transform.localScale.x < 0) {
            transform.position = new Vector3 (transform.localScale.x + transform.position.x, transform.position.y, transform.position.z);
            transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        if (transform.localScale.y < 0) {
            transform.position = new Vector3 (transform.position.x, transform.position.y - transform.localScale.y, transform.position.z);
            transform.localScale = new Vector3 (transform.localScale.x, Mathf.Abs (transform.localScale.y), transform.localScale.z);
        }

        //create each tile and fills the whole collider with them

        for (int i = 0; i < platformLength; i++) {
            for (int j = 0; j < platformHeight; j++) {
                GameObject tile = new GameObject ("Platform Sprite Tile");
                tile.transform.parent = transform;
                tile.transform.position = new Vector3 (i + transform.position.x, -j + transform.position.y, transform.position.z);
                tile.AddComponent<SpriteRenderer> ();
                tile.GetComponent<SpriteRenderer> ().sprite = material;
            }
        }

        //deactivates the placeholder if a sprite material was atached to the platform

        if (material != null) {
            transform.GetChild (0).gameObject.SetActive (false);
        }

        //creates the animator component and moves the platform to aparent gameobject to unfix the platform animated original position
        if (itMoves) {

            GameObject mobilePlatform = new GameObject ("MobilePlatform");
            mobilePlatform.transform.position = transform.position;
            transform.parent = mobilePlatform.transform;
            gameObject.AddComponent<Animator> ();
            Animator animate = GetComponent<Animator> ();
            animate.runtimeAnimatorController = movementController;

            switch (direction) {

                case Directions.vertical:
                    indexDirection = 1;
                    break;
                case Directions.horizontal:
                    indexDirection = 2;
                    break;
                case Directions.diagonal1:
                    indexDirection = 3;
                    break;
                case Directions.diagonal2:
                    indexDirection = 4;
                    break;
            }
            animate.SetInteger ("direction", indexDirection);
        }
    } //close the start method
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
} //closes the class PlatformEditor