using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlatformEditor : MonoBehaviour {

    //////////////////////Dimensions of the rectangular platform in units///////////////

    private int platformLength;
    private int platformHeight;

    /////////////////////////Apperance///////////////
    [Space (5)]
    [Header ("Appearance")]
    [Tooltip ("Here goes the sprite of the material that hte platform's tiles are going to have")]
    public Sprite material;

    /////////////////////////Movement///////////////

    [Space (5)]
    [Header ("Movement")]
    [Tooltip ("Allows the platform to move in a particular direction")]
    public bool itMoves;

    [Space (15)]
    [Range (-35, 35)]
    [Tooltip ("How much units the platform it's going to travel horizontally")]
    public int displacementHorizontalUnits;
    [Range (-25, 25)]
    [Tooltip ("How much units the platform it's going to travel vertically")]
    public int displacementVerticalUnits;

    private Vector3 originalPosition;

    [Space (15)]
    [Range (0, 10)]
    [Tooltip ("How much seconds it will take, before the platform starts its loop")]
    public float delayStart;

    [Range (0.1f, 30)]
    [Tooltip ("How much seconds the platform takes to go from PointA to PointB and vice versa")]
    public float secondsOfMovement;

    [Range (0.1f, 10)]
    [Tooltip ("How much seconds the platform stays stand still in pointA and pointB")]
    public int secondsOfStandBy;
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

        //the ability to teleport sets off if a secondary platform wasn't linked to it
        if (itTeleports && linkedPortal == null) itTeleports = false;

        //Coroutine of movement
        if (itMoves) {

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

            float x = transform.position.x;
            float y = transform.position.y;

            //sets the trajectory of the platform
            bool up = false;
            bool down = false;
            bool left = false;
            bool right = false;

            if (displacementHorizontalUnits < 0) {
                left = true;
                right = false;
            } else if (displacementHorizontalUnits > 0) {
                left = false;
                right = true;
            }

            if (displacementVerticalUnits < 0) {
                down = true;
                up = false;
            } else if (displacementVerticalUnits > 0) {
                down = false;
                up = true;
            }

            //go to destination

            yield return new WaitForSeconds (secondsOfStandBy);

            while (((right && x < originalPosition.x + displacementHorizontalUnits) || (left && x > originalPosition.x + displacementHorizontalUnits)) ||
                ((up && y < originalPosition.y + displacementVerticalUnits) || (down && y > originalPosition.y + displacementVerticalUnits))) {

                yield return new WaitForSeconds (1 / 60);
                x += displacementHorizontalUnits / secondsOfMovement * Time.deltaTime;
                y += displacementVerticalUnits / secondsOfMovement * Time.deltaTime;
                transform.position = new Vector3 (x, y, transform.position.z);
            }

            //fixes the platform position to eliminate unwanted decimal positions

            transform.position = new Vector3 (originalPosition.x + displacementHorizontalUnits, originalPosition.y + displacementVerticalUnits, originalPosition.z);

            //go back to origin position

            yield return new WaitForSeconds (secondsOfStandBy);
            up = !up;
            down = !down;
            left = !left;
            right = !right;

            displacementVerticalUnits *= -1;
            displacementHorizontalUnits *= -1;

            originalPosition = transform.position;

            yield return null;
        }
    } //closes the method move
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

    void OnDrawGizmosSelected () {

        if (!itMoves && !itTeleports) return;

        if (itMoves) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube (new Vector3 (transform.position.x + displacementHorizontalUnits + transform.localScale.x / 2, transform.position.y + displacementVerticalUnits - transform.localScale.y / 2, 4), new Vector3 (transform.localScale.x, transform.localScale.y, 1));
            Gizmos.DrawLine (transform.position, new Vector3 (transform.position.x + displacementHorizontalUnits, transform.position.y + displacementVerticalUnits, transform.position.z));

        }

        if (itTeleports) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere (new Vector3 (transform.position.x + transform.localScale.x / 2, transform.position.y - transform.localScale.y / 2, transform.position.z), 1);

            if (linkedPortal != null) {

                Gizmos.color = new Color (1, 0.5f, 0);
                Gizmos.DrawWireSphere (new Vector3 (linkedPortal.transform.position.x + linkedPortal.transform.localScale.x / 2, linkedPortal.transform.position.y - linkedPortal.transform.localScale.y / 2, linkedPortal.transform.position.z), 1);

            }
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

} //closes the class PlatformEditor