using System.Collections;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class MovingPlatformPlaceholder : MonoBehaviour {
    PlatformEditor platform;
    GameObject platformTrajectory;
    GameObject platformTrajectoryPlaceholderCornerMode;
    GameObject platformTrajectoryPlaceholder;
    void Awake () {

        if (Application.isPlaying) {
            Destroy (this);
        }
        platform = GetComponent<PlatformEditor> ();

    } //closes the awake methods

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    void Update () {

        //reset extra unwanted childs
        if (transform.childCount > 1) {
            SafeDestroyGameObject (transform.GetChild (1).gameObject);
        }

        if (!Application.isEditor) {

            Destroy (this);
            return;
        }

        //creates the placeholder object
        if (platform.itMoves) {
            if (platformTrajectory == null) {

                platformTrajectory = new GameObject ("Platform Trajectory");
                platformTrajectoryPlaceholderCornerMode = new GameObject ("Platform Trajectory Corner Mode Placeholder");
                platformTrajectoryPlaceholder = new GameObject ("Platform Trajectory Placeholder");

                platformTrajectoryPlaceholderCornerMode.transform.parent = platformTrajectory.transform;
                platformTrajectoryPlaceholder.transform.parent = platformTrajectoryPlaceholderCornerMode.transform;
                platformTrajectoryPlaceholder.AddComponent<SpriteRenderer> ();
                platformTrajectoryPlaceholder.GetComponent<SpriteRenderer> ().sprite = platform.material;
                platformTrajectory.transform.parent = gameObject.transform;
            }

            //invert the movement

            if (platform.invertDirection) {
                platform.platformVerticalDirection = Mathf.Abs (platform.displacementVerticalUnits) * -1;
                platform.platformHorizontalDirection = Mathf.Abs (platform.displacementHorizontalUnits) * -1;
            } else {
                platform.platformVerticalDirection = Mathf.Abs (platform.displacementVerticalUnits);
                platform.platformHorizontalDirection = Mathf.Abs (platform.displacementHorizontalUnits);
            }

            //moves the placeholder if the platform moves and which direction was chosen
            switch (platform.direction) {

                case PlatformEditor.Directions.horizontal:
                    platformTrajectory.transform.position = new Vector3 (transform.position.x - (platform.platformHorizontalDirection), transform.position.y, transform.position.z);

                    break;
                case PlatformEditor.Directions.vertical:
                    platformTrajectory.transform.position = new Vector3 (transform.position.x, transform.position.y - (platform.platformVerticalDirection), transform.position.z);
                    break;

                case PlatformEditor.Directions.diagonal1:
                    platformTrajectory.transform.position = new Vector3 (transform.position.x - (platform.platformHorizontalDirection), transform.position.y + (platform.platformVerticalDirection), transform.position.z);

                    break;

                case PlatformEditor.Directions.diagonal2:
                    platformTrajectory.transform.position = new Vector3 (transform.position.x - (platform.platformHorizontalDirection), transform.position.y - (platform.platformVerticalDirection), transform.position.z);

                    break;

            }

            //activates the platform  only if the object is selected
            if (Selection.activeTransform == gameObject.transform) {

                platformTrajectory.SetActive (true);
            } else
                platformTrajectory.SetActive (false);

        } //closes the condition of the platform moves

        //deactivates the placeholder if the platform doesn't move

        if (!platform.itMoves) {
            platformTrajectory = SafeDestroyGameObject (platformTrajectory);

        } //closes the condition of the platform being static

    } //cloese the update method

    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////

    //Destroy Objects in EditMode

    public static T SafeDestroyGameObject<T> (T component) where T : Object {
        if (component != null)
            SafeDestroy (component);
        return null;
    }
    public static T SafeDestroy<T> (T obj) where T : Object {
        if (Application.isEditor)
            Object.DestroyImmediate (obj);
        else
            Object.Destroy (obj);

        return null;
    }

} //closes the MovingPlatformPlaceholder class