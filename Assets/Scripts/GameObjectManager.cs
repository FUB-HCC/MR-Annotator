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
using Vuforia;

public class GameObjectManager : MonoBehaviour {
    public Action<LinkedList<Project>> updateSpeechManagerProjects;
    public GameObject annotationInfoBox;
    private LinkedList<Project> projects = new LinkedList<Project>();
    private bool projListLoadedFinished = false;
    private bool objLoadedFinished = true;
    private bool annLoadedFinished = true;
    private Dictionary<String,AnnotatedObject> spawnedObject = new Dictionary<String, AnnotatedObject>();

    public void SpawnObject(String name, String wikiDataId, Vector3 offset, float scale = 1.0f)
    {
        StartCoroutine(SpawnObject_internal(name, wikiDataId, offset, scale));
    }

    private IEnumerator SpawnObject_internal(String name, String wikiDataId, Vector3 offset, float scale = 1.0f)
    {
        while(!projListLoadedFinished)
        {
            yield return null;
        }
        foreach (Project p in projects)
        {
            if (p.name == name)
            {
                spawnedObject.Add(name, new AnnotatedObject(name));
                GameObject parent = GameObject.Find("Holograms");
                objLoadedFinished = false;
                p.provider.LoadObject(p.id, name, this.transform,scale,offset);
                while (!objLoadedFinished)
                {
                    yield return null;
                }
                annLoadedFinished = false;
                parent.GetComponent<CouchDBWrapper>().LoadAnnotations(p.id, name, this.transform, scale, offset);
                while (!annLoadedFinished)
                {
                    yield return null;
                }

                annLoadedFinished = false;
                parent.GetComponent<WikiDataWrapper>().LoadAnnotations(wikiDataId, name, this.transform, scale, offset);
                while (!annLoadedFinished)
                {
                    yield return null;
                }
                /*parent.GetComponent<DBPediaWrapper>().LoadAnnotations("http://dbpedia.org/resource/Eiffel_Tower", name, this.transform, scale, offset);
                while (!annLoadedFinished)
                {
                    yield return null;
                }*/
            }
        }
    }

    private void ObjectLoaded(KeyValuePair<string,GameObject> obj)
    {
        spawnedObject[obj.Key].setObject(obj.Value);
        objLoadedFinished = true;
    }

    private void AnnotationsLoaded(KeyValuePair<string, Dictionary<string,Annotation>> obj)
    {
        spawnedObject[obj.Key].addAnnotations(obj.Value,annotationInfoBox);
        annLoadedFinished = true;
    }

    private void ProjectListLoaded(LinkedList<Project> projects)
    {
        this.projects = new LinkedList<Project>(this.projects.Concat(projects));
        projListLoadedFinished = true;
        updateSpeechManagerProjects(this.projects);
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

    //Reset all loaded Objects
    public void Clear()
    {
        spawnedObject.Clear();
    }

    //Check whether an object with name already is loaded
    public bool ObjectExists(String name)
    {
        return spawnedObject.ContainsKey(name);
    }

    //Get an Object from the Manager by its name
    public AnnotatedObject GetObject(String name)
    {
        AnnotatedObject obj;
        spawnedObject.TryGetValue(name, out obj);
        return obj;
    }

    public void UpdateAnnotedObject(AnnotatedObject obj)
    {
        if (spawnedObject.ContainsKey(name))
        {
            Debug.Log("GameObjectManager");
        }
    }

    //Updade an already existing Annotation idenfied by id from Object identified with name with a new body text
    public void updateAnnotation(String name,String id,String text)
    {
        Debug.Log("Name "+name);
        if (spawnedObject.ContainsKey(name))
        {
            Debug.Log("GameObjectManager");
            spawnedObject[name].updateAnnotation(id, text);
        }
    }

    //Create a new Annotation for the object idenfied by name at position pos with body text
    public String createAnnotation(String name,Vector3 pos,String text)
    {
        if (spawnedObject.ContainsKey(name))
        {
            return spawnedObject[name].addAnnotation(pos, text, annotationInfoBox);
        }
        return "";
    }

    //Delete an existing annotation
    public void DeleteAnnotation(GameObject annotationAnchor)
    {
        GameObject annotationContainer = annotationAnchor.transform.parent.gameObject;
        String name = annotationContainer.transform.parent.parent.name;
        spawnedObject[name].deleteAnnotation(annotationContainer.name);
        Destroy(annotationContainer);
    }

    //Toggle annotation visability of Object identified by name
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
        //Setup Delegates
        parent.GetComponent<CouchDBWrapper>().ProjectListLoaded += ProjectListLoaded;
        parent.GetComponent<CouchDBWrapper>().ObjectLoaded += ObjectLoaded;
        parent.GetComponent<CouchDBWrapper>().AnnotationsLoaded += AnnotationsLoaded;
        parent.GetComponent<WikiDataWrapper>().ProjectListLoaded += ProjectListLoaded;
        parent.GetComponent<WikiDataWrapper>().ObjectLoaded += ObjectLoaded;
        parent.GetComponent<WikiDataWrapper>().AnnotationsLoaded += AnnotationsLoaded;
        parent.GetComponent<DBPediaWrapper>().ProjectListLoaded += ProjectListLoaded;
        parent.GetComponent<DBPediaWrapper>().ObjectLoaded += ObjectLoaded;
        parent.GetComponent<DBPediaWrapper>().AnnotationsLoaded += AnnotationsLoaded;
        this.projListLoadedFinished = false;
        parent.GetComponent<CouchDBWrapper>().GetProjectList();
        this.projListLoadedFinished = false;
        parent.GetComponent<DBPediaWrapper>().GetProjectList();
        //this.projListLoadedFinished = false;
        //parent.GetComponent<WikiDataWrapper>().GetProjectList();
        /*
        StartCoroutine(SpawnObject_internal2("Head of a satyre", new Vector3(0.0f, 0.0f, +0.2f), 0.001f));
        VuforiaBehaviour.Instance.enabled = false;*/

        //StartCoroutine(SpawnObject_internal2("Eiffel tower", new Vector3(0.0f, 0.0f, +0.2f), 0.001f));
        //spawnedObject.Add("sphenoid bone", new AnnotatedObject("sphenoid bone"));
        //parent.GetComponent<WikiDataWrapper>().LoadObject("http://www.wikidata.org/entity/Q16321", name, this.transform, 0.001f, new Vector3(0.0f, 0.0f, -0.5f));



        //SpawnObject("lamp",new Vector3(0.0f,0.0f,-10.0f));
    }


    // Update is called once per frame
    void Update () {
		
	}
}
