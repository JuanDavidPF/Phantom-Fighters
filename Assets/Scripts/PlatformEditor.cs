using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformEditor : MonoBehaviour {

    //Components
    private BoxCollider2D hitbox;

    //placeHolder
    private Vector3 placeHolderDimensions;
    private Vector3 placeHolderPosition;
    private int platformLength;
    private int platformHeight;

    //Graphics
    public Sprite material;

    // Start is called before the first frame update
    void Start () {

        //extract the dimensions and position of the placeholder modified in editor view.
        placeHolderDimensions = transform.GetChild (0).GetComponent<Transform> ().localScale;
        placeHolderPosition = transform.GetChild (0).GetComponent<Transform> ().position;

        platformLength = (int) Mathf.Round (placeHolderDimensions.x);
        platformHeight = (int) Mathf.Round (placeHolderDimensions.y);

        //adjust the hitbox to the dimensions of the placeholder

        hitbox = GetComponent<BoxCollider2D> ();
        hitbox.size = new Vector2 (platformLength, platformHeight);
        hitbox.offset = new Vector2 (platformLength / 2f, -(platformHeight / 2f));

        //create the tiles and fill the hitbox with them

        for (int i = 0; i < platformLength; i++) {
            for (int j = 0; j < platformHeight; j++) {
                GameObject tile = new GameObject ("Platform Sprite Tile");
                tile.transform.parent = transform;
                tile.transform.position = new Vector3 (i + transform.position.x, -j + transform.position.y, transform.position.z);
                tile.AddComponent<SpriteRenderer> ();
                tile.GetComponent<SpriteRenderer> ().sprite = material;
            }
        }

        //deactivates the placeholder
        transform.GetChild (0).gameObject.SetActive (false);
    } //close the start method

    // Update is called once per frame

}