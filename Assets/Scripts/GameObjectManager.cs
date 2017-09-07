using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows.Speech;


public class GameObjectManager : MonoBehaviour {
    private ProjectInfoJson projects;
    private Dictionary<String,AnnotatedObject> spawnedObject = new Dictionary<String, AnnotatedObject>();

    public void SpawnObject(String name)
    {
        foreach (ProjectJson p in projects.projects)
        {
            if (p.name == name)
            {
                GameObject parent = GameObject.Find("Holograms");
                AnnotatedObject obj = parent.GetComponent<CouchDBWrapper>().LoadObject(p._id, this.transform);
                obj.annotatedObject.name = name;
                spawnedObject.Add(name,obj);
                break;
            }
        }
    }

    public void DeleteObject(String name)
    {
        Debug.Log("Delete " + name);
        if (spawnedObject.ContainsKey(name))
        {
            Destroy(spawnedObject[name].annotatedObject);
            spawnedObject[name] = null;
            spawnedObject.Remove(name);
        }
    }

    public void updateAnnotation(String name,String id,String text)
    {
        Debug.Log("Name "+name);
        if (spawnedObject.ContainsKey(name))
        {
            Debug.Log("GameObjectManager");
            spawnedObject[name].updateAnnotation(id, text);
        }
    }

    public String createAnnotation(String name,Vector3 pos,String text)
    {
        if (spawnedObject.ContainsKey(name))
        {
            return spawnedObject[name].addAnnotation(pos, text);
        }
        return "";
    }

    public void DeleteAnnotation(GameObject annotationAnchor)
    {
        GameObject annotationContainer = annotationAnchor.transform.parent.gameObject;
        String name = annotationContainer.transform.parent.parent.name;
        spawnedObject[name].deleteAnnotation(annotationContainer.name);
        Destroy(annotationContainer);
    }

    public void hideAnnotations(String name,bool hide)
    {
        if (spawnedObject.ContainsKey(name))
        {
            spawnedObject[name].hideAnnotations(hide);
        }
    }

    // Use this for initialization
    void Start () {
        
        GameObject parent = GameObject.Find("Holograms");
        projects = parent.GetComponent<CouchDBWrapper>().GetProjectList();
        SpawnObject("lamp");
    }


	// Update is called once per frame
	void Update () {
		
	}
}
