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
using ff.vr.annotate.viz;


public class GameObjectManager : MonoBehaviour {
    private ProjectInfoJson projects;
    private GameObject spawnedObjects;
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
                Bounds aabb = obj.annotatedObject.GetComponent<Renderer>().bounds;
                Vector3 max = aabb.max;
                Vector3 min = aabb.min;
                Vector3 scaleFactors = new Vector3();
                scaleFactors.x = 1.0f / (max.x - min.x);
                scaleFactors.y = 1.0f / (max.y - min.y);
                scaleFactors.z = 1.0f / (max.z - min.z);
                if(scaleFactors.x < scaleFactors.y && scaleFactors.x < scaleFactors.z)
                {
                    obj.annotatedObject.transform.localScale = new Vector3(scaleFactors.x, scaleFactors.x, scaleFactors.x);
                }
                else if(scaleFactors.y < scaleFactors.x && scaleFactors.y < scaleFactors.z)
                {
                    obj.annotatedObject.transform.localScale = new Vector3(scaleFactors.y, scaleFactors.y, scaleFactors.y);
                }
                else
                {
                    obj.annotatedObject.transform.localScale = new Vector3(scaleFactors.z, scaleFactors.z, scaleFactors.z);
                }
                obj.annotatedObject.transform.localScale *= 5.0f;
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
        //AnnotationManager.Instance = new AnnotationManager();
        GameObject parent = GameObject.Find("Holograms");
        projects = parent.GetComponent<CouchDBWrapper>().GetProjectList();
        SpawnObject("lamp");
    }


	// Update is called once per frame
	void Update () {
		
	}
}
