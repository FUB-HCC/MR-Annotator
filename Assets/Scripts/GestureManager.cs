using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

public class GestureManager : MonoBehaviour {
    public enum ManipulationMode{
        MODE_ROTATE,
        MODE_SCALE
    };
    private GameObject FocusedObject = null;
    private GameObject manipulationObject = null;
    private GestureRecognizer gestureRecognizer = null;
    private bool manipulation = false;
    public ManipulationMode mode = ManipulationMode.MODE_SCALE;
	// Use this for initialization
	void Start () {
        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.ManipulationStartedEvent += ManipulationStarted;
        gestureRecognizer.ManipulationUpdatedEvent += ManipulationUpdated;
        gestureRecognizer.ManipulationCompletedEvent += ManipulationCompleted;
        gestureRecognizer.SetRecognizableGestures(GestureSettings.ManipulationTranslate);
        gestureRecognizer.StartCapturingGestures();
	}

    private void ManipulationCompleted(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        manipulation = false;
        manipulationObject = null;
    }

    private void ManipulationStarted(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        manipulation = true;
        manipulationObject = FocusedObject;
    }

    private void ManipulationUpdated(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        if(manipulationObject!=null)
        {
            if (mode == ManipulationMode.MODE_ROTATE)
            {
                Vector3 axis = new Vector3(cumulativeDelta.y, cumulativeDelta.x, cumulativeDelta.z);
                manipulationObject.transform.Rotate(axis.normalized, 0.1f, Space.Self);
            }
            else if(mode == ManipulationMode.MODE_SCALE)
            {
                float scale = cumulativeDelta.x;
                Vector3 currentScale = manipulationObject.transform.localScale;
                manipulationObject.transform.localScale = new Vector3(currentScale.x+scale, currentScale.y + scale, currentScale.z + scale);
            }
        }
        Debug.Log(cumulativeDelta);
    }

    // Update is called once per frame
    void Update () {
        if (!manipulation)
        {
            // Figure out which hologram is focused this frame.
            GameObject oldFocusObject = FocusedObject;

            // Do a raycast into the world based on the user's
            // head position and orientation.
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;

            RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
            {
                // If the raycast hit a hologram, use that as the focused object.
                FocusedObject = hitInfo.collider.gameObject;
            }
            else
            {
                // If the raycast did not hit a hologram, clear the focused object.
                FocusedObject = null;
            }

            // If the focused object changed this frame,
            // start detecting fresh gestures again.
            if (FocusedObject != oldFocusObject)
            {
                gestureRecognizer.CancelGestures();
                gestureRecognizer.StartCapturingGestures();
            }
        }
    }
}
