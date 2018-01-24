using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ProjectInfoJson
{
    public String _id;
    public String _rev;
    public ProjectJson[] projects;
}

[System.Serializable]
public class ProjectJson
{
    public String _id;
    public String name;
    public String activeTopic;
}

[System.Serializable]
public class AnnotationReponseValueJson
{
    public string id;
    public string key;
    public Annotation doc;
}

[System.Serializable]
public class AnnotationResponseJson
{
    public int total_rows;
    public int offset;
    public AnnotationReponseValueJson[] rows;
}

public class CouchDBWrapper : MonoBehaviour {
    public string username;
    public string password;
    public string url;

    public ProjectInfoJson projects;

    private string GetRequest(string uri)
    {
        string authorization = username + ":" + password;
        byte[] binaryAuthorization = System.Text.Encoding.UTF8.GetBytes(authorization);
        authorization = Convert.ToBase64String(binaryAuthorization);
        UnityWebRequest request = UnityWebRequest.Get(url+uri);
        request.SetRequestHeader("AUTHORIZATION", "Basic " + authorization);
        request.Send();
        while(!request.isDone)
        {
        }

        // Show results as text
        String text = request.downloadHandler.text;
        Debug.Log(text);
        return text;
    }

    public AnnotatedObject LoadObject(string id,Transform parent)
    {
        string text = GetRequest("/" + id + "/topic_/file");
        Debug.Log(text);

        ObjImporter importer = new ObjImporter();
        GameObject obj = new GameObject();
        obj.transform.parent = parent;
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshCollider>();
        obj.GetComponent<MeshFilter>().mesh = importer.ImportFile(text);
        obj.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        obj.GetComponent<MeshFilter>().mesh.RecalculateTangents();
        obj.GetComponent<MeshFilter>().mesh.RecalculateBounds();
        obj.GetComponent<MeshCollider>().sharedMesh = obj.GetComponent<MeshFilter>().mesh;
        obj.GetComponent<Renderer>().enabled = true;
        obj.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/defaultMat");

        text = GetRequest("/" + id + "/_all_docs?include_docs=true");

        AnnotationResponseJson resp = JsonUtility.FromJson<AnnotationResponseJson>(text);
        Dictionary<String,Annotation> annotations = new Dictionary<String,Annotation>();
        foreach (AnnotationReponseValueJson r in resp.rows)
        {
            if (r.doc._id != "info" && r.doc._id != "topic_")
            {
                annotations.Add(r.doc._id,r.doc);
            }
        }

        return new AnnotatedObject(obj, annotations);
    }

    public ProjectInfoJson GetProjectList()
    {
        string text = GetRequest("/info/projectsInfo");
        projects = JsonUtility.FromJson<ProjectInfoJson>(text);
        Debug.Log(text);
        Debug.Log(projects.projects[0]._id);
        return projects;
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
