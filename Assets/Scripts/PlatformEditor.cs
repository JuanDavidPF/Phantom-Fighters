using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlatformEditor : MonoBehaviour {

    //////////////////////Dimensions of the rectangular platform in units///////////////

    private int platformLength;
    private int platformHeight;

    /////////////////////////Variables with predifined values///////////////
    public enum Directions { horizontal, vertical, diagonal1, diagonal2 }

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
    public Directions direction = Directions.horizontal;

    [Tooltip ("Reflects the axis of the movement")]
    public bool invertDirection;

    [Space (15)]
    [Range (1, 35)]
    [Tooltip ("How much units the platform it's going to travel horizontally")]
    public float displacementHorizontalUnits = 7;
    [Range (1, 30)]
    [Tooltip ("How much units the platform it's going to travel vertically")]
    public float displacementVerticalUnits = 3;
    [HideInInspector]
    public float platformHorizontalDirection;
    [HideInInspector]
    public float platformVerticalDirection;
    private float clock;
    private float seconds;
    private float countDown;
    private string movementPhase;
    private Vector3 originalPosition;

    [Space (15)]

    [Range (1, 10)]
    [Tooltip ("How much seconds will take, before the platform starts its loop")]
    public float delayStart = 1;

    [Range (1, 30)]
    [Tooltip ("How much seconds the platform takes to go from PointA to PointB and vice versa")]
    public float secondsOfMovement = 10;

    [Range (1, 10)]
    [Tooltip ("How much seconds the platform stays stand still in pointA and pointB")]
    public float secondsOfStandBy = 3;

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
            Destroy (transform.GetChild (0).gameObject);
        }

        //deactivates the placeholder of movement if the platform moves

        if (itMoves) {
            Destroy (transform.GetChild (1).gameObject);
        }

        //saves the original position of the platform

        originalPosition = transform.position;

        //initialize the movement phase
        movementPhase = "AtoB";

        //The delay start is aplied
        clock -= delayStart;

    } //close the start method

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void Update () {

        //manages the seconds counter

        clock += Time.deltaTime;
        seconds = (int) Mathf.Round (clock % 60);

        if (itMoves) {

            Move ();

        } //closes the condition of moving
    } //closes the method Update

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void Move () {

        switch (direction) {

            case Directions.horizontal:

                switch (movementPhase) {

                    case "AtoB":

                        if (seconds >= secondsOfStandBy) {

                            float x = transform.position.x;
                            if (!invertDirection && x > originalPosition.x - platformHorizontalDirection) x -= (platformHorizontalDirection / secondsOfMovement * Time.deltaTime);
                            else if (invertDirection && x < originalPosition.x - platformHorizontalDirection) x -= (platformHorizontalDirection / secondsOfMovement * Time.deltaTime);

                            transform.position = new Vector3 (x, transform.position.y, transform.position.z);

                            if (seconds >= secondsOfStandBy + secondsOfMovement) {
                                clock = 0;
                                movementPhase = "BtoA";
                            }
                        }
                        break;

                    case "BtoA":

                        if (seconds >= secondsOfStandBy) {

                            float x = transform.position.x;
                            if (!invertDirection && x < originalPosition.x) x += platformHorizontalDirection / secondsOfMovement * Time.deltaTime * 1f;
                            else if (invertDirection && x > originalPosition.x) x += platformHorizontalDirection / secondsOfMovement * Time.deltaTime * 1f;

                            transform.position = new Vector3 (x, transform.position.y, transform.position.z);
                            if (seconds >= secondsOfStandBy + secondsOfMovement) {
                                clock = 0;
                                movementPhase = "AtoB";
                            }
                        }
                        break;

                } //closes the switch of the phase

                break;

            case Directions.vertical:

                switch (movementPhase) {

                    case "AtoB":

                        if (seconds >= secondsOfStandBy) {

                            float y = transform.position.y;
                            if (!invertDirection && y > originalPosition.y - platformVerticalDirection) y -= (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            else if (invertDirection && y < originalPosition.y - platformVerticalDirection) y -= (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            transform.position = new Vector3 (transform.position.x, y, transform.position.z);

                            if (seconds >= secondsOfStandBy + secondsOfMovement) {
                                clock = 0;
                                movementPhase = "BtoA";
                            }
                        }
                        break;

                    case "BtoA":

                        if (seconds >= secondsOfStandBy) {

                            float y = transform.position.y;
                            if (!invertDirection && y < originalPosition.y) y += platformVerticalDirection / secondsOfMovement * Time.deltaTime;
                            else if (invertDirection && y > originalPosition.y) y += platformVerticalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (transform.position.x, y, transform.position.z);

                            if (seconds >= secondsOfStandBy + secondsOfMovement) {
                                clock = 0;
                                movementPhase = "AtoB";
                            }
                        }
                        break;

                } //closes the switch of the phase

                break;

            case Directions.diagonal1:

                switch (movementPhase) {

                    case "AtoB":

                        if (seconds >= secondsOfStandBy) {

                            float x = transform.position.x;
                            if (!invertDirection && x > originalPosition.x - platformHorizontalDirection) x -= (platformHorizontalDirection / secondsOfMovement * Time.deltaTime);
                            else if (invertDirection && x < originalPosition.x - platformHorizontalDirection) x -= (platformHorizontalDirection / secondsOfMovement * Time.deltaTime);
                            transform.position = new Vector3 (x, transform.position.y, transform.position.z);

                            float y = transform.position.y;
                            if (!invertDirection && y > originalPosition.y - platformVerticalDirection) y += (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            else if (invertDirection && y < originalPosition.y - platformVerticalDirection) y += (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            transform.position = new Vector3 (transform.position.x, y, transform.position.z);

                            if (seconds >= secondsOfStandBy + secondsOfMovement) {
                                clock = 0;
                                movementPhase = "BtoA";
                            }
                        }
                        break;

                    case "BtoA":

                        if (seconds >= secondsOfStandBy) {

                            float x = transform.position.x;
                            if (!invertDirection && x < originalPosition.x) x += platformHorizontalDirection / secondsOfMovement * Time.deltaTime;
                            else if (invertDirection && x > originalPosition.x) x += platformHorizontalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (x, transform.position.y, transform.position.z);

                            float y = transform.position.y;
                            if (!invertDirection && y > originalPosition.y) y -= platformVerticalDirection / secondsOfMovement * Time.deltaTime;
                            else if (invertDirection && y < originalPosition.y) y -= platformVerticalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (transform.position.x, y, transform.position.z);

                            if (seconds >= secondsOfStandBy + secondsOfMovement) {
                                clock = 0;
                                movementPhase = "AtoB";
                            }
                        }
                        break;

                } //closes the switch of the phase

                break;

            case Directions.diagonal2:

                switch (movementPhase) {

                    case "AtoB":

                        if (seconds >= secondsOfStandBy) {

                            float x = transform.position.x;
                            if (!invertDirection && x > originalPosition.x - platformHorizontalDirection) x -= (platformHorizontalDirection / secondsOfMovement * Time.deltaTime);
                            else if (invertDirection && x < originalPosition.x - platformHorizontalDirection) x -= (platformHorizontalDirection / secondsOfMovement * Time.deltaTime);
                            transform.position = new Vector3 (x, transform.position.y, transform.position.z);

                            float y = transform.position.y;
                            if (!invertDirection && y > originalPosition.y - platformVerticalDirection) y -= (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            else if (invertDirection && y < originalPosition.y - platformVerticalDirection) y -= (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            transform.position = new Vector3 (transform.position.x, y, transform.position.z);

                            if (seconds >= secondsOfStandBy + secondsOfMovement) {
                                clock = 0;
                                movementPhase = "BtoA";
                            }
                        }
                        break;

                    case "BtoA":

                        if (seconds >= secondsOfStandBy) {

                            float x = transform.position.x;
                            if (!invertDirection && x < originalPosition.x) x += platformHorizontalDirection / secondsOfMovement * Time.deltaTime;
                            else if (invertDirection && x > originalPosition.x) x += platformHorizontalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (x, transform.position.y, transform.position.z);

                            float y = transform.position.y;
                            if (!invertDirection && y < originalPosition.y) y += platformVerticalDirection / secondsOfMovement * Time.deltaTime;
                            else if (invertDirection && y > originalPosition.y) y += platformVerticalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (transform.position.x, y, transform.position.z);

                            if (seconds >= secondsOfStandBy + secondsOfMovement) {
                                clock = 0;
                                movementPhase = "AtoB";
                            }
                        }
                        break;

                } //closes the switch of the phase

                break;

        } //closes the switch of the direction

    }

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

} //closes the class PlatformEditor