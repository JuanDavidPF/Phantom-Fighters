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

    private Vector3 originalPosition;

    [Space (15)]
    [Range (0, 10)]
    [Tooltip ("How much seconds will take, before the platform starts its loop")]
    public float delayStart;

    [Range (0, 30)]
    [Tooltip ("How much seconds the platform takes to go from PointA to PointB and vice versa")]
    public float secondsOfMovement;

    [Range (0, 10)]
    [Tooltip ("How much seconds the platform stays stand still in pointA and pointB")]
    public float secondsOfStandBy;
    private IEnumerator movementCoroutine;
    /////////////////////////Teleportation///////////////

    [Space (5)]
    [Header ("Teleportation")]
    [Tooltip ("Allows this platform to teleport")]
    public bool itTeleports;

    [Tooltip ("Link to the destination platform")]
    public GameObject linkedPortal;

    [Tooltip ("How many seconds the player has to stand in the platform before he gets teleported")]
    [Range (0, 5)]
    public float portalCooldown;
    private bool isSending;
    private bool isReceiving;
    private GameObject playerTeleporting;
    private IEnumerator teleportCoroutine;

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    IEnumerator Start () {

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

        //the ability to teleport sets off if a secondary platform wasn't linked to it. Else, it linkes itself to the other platform
        if (itTeleports && linkedPortal == null) itTeleports = false;

        //Coroutine of movement
        if (itMoves) {

            //deactivates the placeholder of movement if the platform moves
            Destroy (transform.GetChild (1).gameObject);

            //saves the original position of the platform
            originalPosition = transform.position;
            movementCoroutine = Move ();
            yield return new WaitForSeconds (delayStart);
            StartCoroutine (movementCoroutine);
        }

        yield return null;

    } //close the start method

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    IEnumerator Move () {

        while (itMoves) {

            yield return new WaitForSeconds (secondsOfStandBy);

            float x = transform.position.x;
            float y = transform.position.y;

            switch (direction) {

                case Directions.horizontal:

                    if (!invertDirection) {

                        while (x > originalPosition.x - platformHorizontalDirection) {
                            yield return new WaitForSeconds (1 / 60);
                            x = transform.position.x;
                            x -= platformHorizontalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (x, transform.position.y, transform.position.z);
                        }
                    } else if (invertDirection)
                        while (x < originalPosition.x - platformHorizontalDirection) {
                            yield return new WaitForSeconds (1 / 60);
                            x = transform.position.x;
                            x -= platformHorizontalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (x, transform.position.y, transform.position.z);
                        }

                    yield return new WaitForSeconds (secondsOfStandBy);

                    if (!invertDirection)
                        while (x < originalPosition.x) {
                            yield return new WaitForSeconds (1 / 60);
                            x = transform.position.x;
                            x += platformHorizontalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (x, transform.position.y, transform.position.z);
                        }
                    else if (invertDirection)
                        while (x > originalPosition.x) {
                            yield return new WaitForSeconds (1 / 60);
                            x = transform.position.x;
                            x += platformHorizontalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (x, transform.position.y, transform.position.z);
                        }
                    break;

                case Directions.vertical:

                    if (!invertDirection) {

                        while (y > originalPosition.y - platformVerticalDirection) {
                            yield return new WaitForSeconds (1 / 60);
                            y = transform.position.y;
                            y -= (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            transform.position = new Vector3 (transform.position.x, y, transform.position.z);
                        }
                    } else if (invertDirection)
                        while (y < originalPosition.y - platformVerticalDirection) {
                            yield return new WaitForSeconds (1 / 60);
                            y = transform.position.y;
                            y -= platformVerticalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (transform.position.x, y, transform.position.z);
                        }

                    yield return new WaitForSeconds (secondsOfStandBy);

                    if (!invertDirection)
                        while (y < originalPosition.y) {
                            yield return new WaitForSeconds (1 / 60);
                            y = transform.position.y;
                            y += platformVerticalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (transform.position.x, y, transform.position.z);
                        }
                    else if (invertDirection)
                        while (y > originalPosition.y) {
                            y = transform.position.y;
                            yield return new WaitForSeconds (1 / 60);
                            y += platformVerticalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (transform.position.x, y, transform.position.z);
                        }
                    break;

                case Directions.diagonal1:

                    if (!invertDirection) {

                        while (x > originalPosition.x - platformHorizontalDirection && y > originalPosition.y - platformVerticalDirection) {
                            yield return new WaitForSeconds (1 / 60);
                            x = transform.position.x;
                            y = transform.position.y;
                            x -= (platformHorizontalDirection / secondsOfMovement * Time.deltaTime);
                            y += (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            transform.position = new Vector3 (x, y, transform.position.z);
                        }
                    } else if (invertDirection) {
                        while (x < originalPosition.x - platformHorizontalDirection && y < originalPosition.y - platformVerticalDirection) {
                            yield return new WaitForSeconds (1 / 60);
                            x = transform.position.x;
                            y = transform.position.y;
                            x -= (platformHorizontalDirection / secondsOfMovement * Time.deltaTime);
                            y += (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            transform.position = new Vector3 (x, y, transform.position.z);
                        }
                    }

                    yield return new WaitForSeconds (secondsOfStandBy);

                    if (!invertDirection) {

                        while (x < originalPosition.x && y > originalPosition.y) {
                            yield return new WaitForSeconds (1 / 60);
                            x = transform.position.x;
                            y = transform.position.y;
                            x += platformHorizontalDirection / secondsOfMovement * Time.deltaTime;
                            y -= platformVerticalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (x, y, transform.position.z);
                        }
                    } else if (invertDirection) {
                        while (x > originalPosition.x && y < originalPosition.y) {
                            yield return new WaitForSeconds (1 / 60);
                            x = transform.position.x;
                            y = transform.position.y;
                            x += platformHorizontalDirection / secondsOfMovement * Time.deltaTime;
                            y -= platformVerticalDirection / secondsOfMovement * Time.deltaTime;
                            transform.position = new Vector3 (x, y, transform.position.z);
                        }
                    }

                    break;

                case Directions.diagonal2:

                    if (!invertDirection) {
                        while (x > originalPosition.x - platformHorizontalDirection && y > originalPosition.y - platformVerticalDirection) {
                            yield return new WaitForSeconds (1 / 60);
                            x = transform.position.x;
                            y = transform.position.y;
                            x -= (platformHorizontalDirection / secondsOfMovement * Time.deltaTime);
                            y -= (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            transform.position = new Vector3 (x, y, transform.position.z);
                        }
                    } else if (invertDirection) {
                        while (x < originalPosition.x - platformHorizontalDirection && y < originalPosition.y - platformVerticalDirection) {
                            yield return new WaitForSeconds (1 / 60);
                            x = transform.position.x;
                            y = transform.position.y;
                            x -= (platformHorizontalDirection / secondsOfMovement * Time.deltaTime);
                            y -= (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            transform.position = new Vector3 (x, y, transform.position.z);
                        }
                    }

                    yield return new WaitForSeconds (secondsOfStandBy);

                    if (!invertDirection) {
                        while (x < originalPosition.x && y < originalPosition.y) {
                            yield return new WaitForSeconds (1 / 60);
                            x = transform.position.x;
                            y = transform.position.y;
                            x += (platformHorizontalDirection / secondsOfMovement * Time.deltaTime);
                            y += (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            transform.position = new Vector3 (x, y, transform.position.z);
                        }
                    } else if (invertDirection) {
                        while (x > originalPosition.x && y > originalPosition.y) {
                            yield return new WaitForSeconds (1 / 60);
                            x = transform.position.x;
                            y = transform.position.y;
                            x += (platformHorizontalDirection / secondsOfMovement * Time.deltaTime);
                            y += (platformVerticalDirection / secondsOfMovement * Time.deltaTime);
                            transform.position = new Vector3 (x, y, transform.position.z);
                        }
                    }
                    break;
            } //cierra el switch

            yield return null;
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    IEnumerator Teleport (GameObject player) {

        isSending = true;
        linkedPortal.GetComponent<PlatformEditor> ().isReceiving = true;

        yield return new WaitForSeconds (portalCooldown);

        isSending = false;
        linkedPortal.GetComponent<PlatformEditor> ().isReceiving = false;
        player.transform.position = new Vector3 (linkedPortal.transform.position.x + linkedPortal.transform.localScale.x / 2, linkedPortal.transform.position.y, linkedPortal.transform.position.z);

        yield return new WaitForSeconds (portalCooldown);

        yield return null;

    } //closes teleport

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void OnTriggerEnter2D (Collider2D collider) {

        if (itTeleports) {

            if (playerTeleporting == null && collider.gameObject.CompareTag ("Player") && !isReceiving && linkedPortal.GetComponent<PlatformEditor> ().isSending == false) {
                playerTeleporting = collider.gameObject;
            } else if (playerTeleporting != collider.gameObject && collider.gameObject.CompareTag ("Player") && !isSending && !isReceiving) {
                playerTeleporting = collider.gameObject;
            }

            if (playerTeleporting == collider.gameObject) {

                teleportCoroutine = Teleport (playerTeleporting);
                StartCoroutine (teleportCoroutine);

            }
        }

    } //closes method OnTrigger enter

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void OnTriggerExit2D (Collider2D collider) {

        if (playerTeleporting == collider.gameObject && itTeleports && isSending) {
            linkedPortal.GetComponent<PlatformEditor> ().isReceiving = false;
            isSending = false;
            playerTeleporting = null;
            isReceiving = false;

            StopCoroutine (teleportCoroutine);

        }
    } //closes method OnTrigger exit
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

} //closes the class PlatformEditor