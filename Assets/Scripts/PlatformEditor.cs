using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformEditor : MonoBehaviour {

    //Components
    private BoxCollider2D hitbox;

    //Hitbox
    private Vector2 hitboxDimensions;
    private Vector2 hitboxPosition;

    //Graphics

    public GameObject material;

    public int platformLength;

    // Start is called before the first frame update
    void Start () {

        hitboxDimensions = new Vector2 (platformLength, 1);
        hitboxPosition = new Vector2 (platformLength / 2f, -0.5f);

        hitbox = GetComponent<BoxCollider2D> ();
        hitbox.size = hitboxDimensions;
        hitbox.offset = hitboxPosition;

        //create tiles   

        for (int i = 0; i < platformLength; i++) {
            GameObject tile = Instantiate (material);
            tile.transform.parent = transform;
            Vector3 tileSortPosition = new Vector3 (i + transform.position.x, transform.position.y, transform.position.z);
            tile.transform.position = tileSortPosition;
        }

    } //close the start method

    // Update is called once per frame

}