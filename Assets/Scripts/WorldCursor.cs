using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;
using ff.utils;

public class WorldCursor : Singleton<WorldCursor> {
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
            GameObject surface = GameObject.Find("SpatialSurfaceManager");
            if(mode == ManipulationMode.MODE_MOVE && value== ManipulationMode.MODE_LOOK)
            {
                surface.GetComponent<SpatialMappingRenderer>().renderState = SpatialMappingRenderer.RenderState.Occlusion;
                surface.GetComponent<SpatialMappingCollider>().enabled = false;
            }
            if (mode == ManipulationMode.MODE_LOOK && value == ManipulationMode.MODE_MOVE)
            {
                surface.GetComponent<SpatialMappingRenderer>().renderState = SpatialMappingRenderer.RenderState.Visualization;
                surface.GetComponent<SpatialMappingCollider>().enabled = true;
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
            RaycastHit hit;
            //Debug.Log(hits.Length);
            if (Physics.Raycast(headPosition, gazeDirection,out hit, 1 << LayerMask.NameToLayer("SpatialSurfaces")))
            {
                //if()
                if (hit.collider.gameObject.name.Contains("spatial-mapping-surface"))
                {
                    activeSelection.transform.position = hit.point;
                    Debug.Log(hit.collider.gameObject.name);
                    Debug.Log(hit.point);
                }
            }
            else
            {
                activeSelection.transform.position = (headPosition + 10.0f * gazeDirection);
            }
        }
	}
    
    public Vector3 GetPosition()
    {
        return Camera.main.transform.position;
    }
    public Quaternion GetRotation()
    {
        return Camera.main.transform.rotation;
    }
    public Vector3 LookAtDirection()
    {
        return Camera.main.transform.forward;
    }
}
