using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlatformEditor : MonoBehaviour {

    private int platformLength;
    private int platformHeight;

    [Header ("Appearance")]
    public Sprite material;

    // Start is called before the first frame update
    void Start () {

        platformLength = (int) Mathf.Round (Mathf.Abs (transform.localScale.x));
        platformHeight = (int) Mathf.Round (Mathf.Abs (transform.localScale.y));

        //fixes the position of the platform if is negative scaled
        if (transform.localScale.x < 0) {
            transform.position = new Vector3 (transform.localScale.x + transform.position.x, transform.position.y, transform.position.z);
            transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        if (transform.localScale.y < 0) {
            transform.position = new Vector3 (transform.position.x, transform.position.y - transform.localScale.y, transform.position.z);
            transform.localScale = new Vector3 (transform.localScale.x, Mathf.Abs (transform.localScale.y), transform.localScale.z);
        }

        //create the tiles and fill the platform with them

        for (int i = 0; i < platformLength; i++) {
            for (int j = 0; j < platformHeight; j++) {
                GameObject tile = new GameObject ("Platform Sprite Tile");
                tile.transform.parent = transform;
                tile.transform.position = new Vector3 (i + transform.position.x, -j + transform.position.y, transform.position.z);
                tile.AddComponent<SpriteRenderer> ();
                tile.GetComponent<SpriteRenderer> ().sprite = material;
            }
        }

        //deactivates the placeholder if there is a material set to it

        if (material != null) {
            transform.GetChild (0).gameObject.SetActive (false);
        }

    } //close the start method

}