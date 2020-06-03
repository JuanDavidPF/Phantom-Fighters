using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBoundsDetection : MonoBehaviour {

    //detecta a que se un jugador se haya salido de los limites
    void OnTriggerEnter2D (Collider2D player) {

        if (player.gameObject.CompareTag ("Player")) {

            PlayerScript playerParameters = player.GetComponent<PlayerScript> ();

            playerParameters.isOutOfBounds = true;

        }

    } //closes method OnTrigger enter
} //closes the class outofbounds