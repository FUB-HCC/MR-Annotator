using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class MarkerAugmentation : MonoBehaviour {
    public Vector3[,] trackerPos = new Vector3[2, 2];
    public bool[,] trackerState = new bool[2,2] {{false,false},{false,false}};
    public string trackedObject = "";
    public string wikiDataId = "";

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        if ((trackerState[0, 0] && trackerState[1, 1]))
        {
            if (!GameObject.Find("Holograms").GetComponent<GameObjectManager>().ObjectExists(trackedObject))
            {
                GameObject.Find("Holograms").GetComponent<GameObjectManager>().SpawnObject(trackedObject, wikiDataId, trackerPos[0, 0] + 0.5f * (trackerPos[1, 1] - trackerPos[0, 0]), 0.001f);
                VuforiaBehaviour.Instance.enabled = false;
            }
        }
        else if(trackerState[0, 1] && trackerState[1, 0])
        {
            if (!GameObject.Find("Holograms").GetComponent<GameObjectManager>().ObjectExists(trackedObject))
            {
                GameObject.Find("Holograms").GetComponent<GameObjectManager>().SpawnObject(trackedObject, wikiDataId, trackerPos[0, 1] + 0.5f * (trackerPos[1, 0] - trackerPos[0, 1]), 0.001f);
                VuforiaBehaviour.Instance.enabled = false;
            }
        }
        else
        {
            //GameObject.Find("Holograms").GetComponent<GameObjectManager>().DeleteObject(trackedObject);
            //target.SetActiveRecursively(false);
        }
    }
}
