using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;

public class WorldCursor : MonoBehaviour {
    public enum ManipulationMode
    {
        MODE_LOOK,
        MODE_MOVE
    };
    private SurfaceObserver surfaceObserver = null;

    public Vector3 lookAtPoint;
    private MeshRenderer renderer;
    private GameObject activeSelection;
    private ManipulationMode mode = ManipulationMode.MODE_LOOK;

    public GameObject ActiveSelection
    {
        get
        {
            return activeSelection;
        }
    }

    public ManipulationMode Mode
    {
        get
        {
            return mode;
        }
        set
        {
            GameObject surface = GameObject.Find("SpatialSurface");
            if(mode == ManipulationMode.MODE_MOVE && value== ManipulationMode.MODE_LOOK)
            {
                surface.SetActive(false);
            }
            if (mode == ManipulationMode.MODE_LOOK && value == ManipulationMode.MODE_MOVE)
            {
                surface.SetActive(true);
            }
            mode = value;
        }
    }

    // Use this for initialization
    void Start () {
        renderer = this.GetComponentInChildren<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 headPosition = Camera.main.transform.position;
        Vector3 gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;
        if (mode == ManipulationMode.MODE_LOOK)
        {
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
            {
                renderer.enabled = true;
                activeSelection = hitInfo.collider.gameObject;
                lookAtPoint = hitInfo.point;
                this.transform.position = hitInfo.point;
                this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }
            else
            {
                activeSelection = null;
                renderer.enabled = false;
            }
        }
        else
        {
            activeSelection.transform.position = (headPosition + 10.0f*gazeDirection);
        }
	}
}
