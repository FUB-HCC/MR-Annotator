﻿using System;
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

[System.Serializable]
public class TopicResponseJson
{
    public string _id;
    public string _rev;
    public string fileName;
    public string fileEnding;
    public AttachmentResponseJson[] _attachments;
}

[System.Serializable]
public class FileResponseJson
{
    public string content_type;
    public int revpos;
    public string digest;
    public int length;
    public bool stub;
}

[System.Serializable]
public class AttachmentResponseJson
{
    public FileResponseJson file;
}

/**
 *Class that Wraps the communication of the ProcessAnnotator's CouchDB  
 */
public class CouchDBWrapper : IDataService {
    public string username;
    public string password;
    public string url;

    public ProjectInfoJson projects;

    //Create a Get Request for a specified uri
    private UnityWebRequest CreateGetRequest(string uri,bool data=false)
    {
        string authorization = username + ":" + password;
        byte[] binaryAuthorization = System.Text.Encoding.UTF8.GetBytes(authorization);
        authorization = Convert.ToBase64String(binaryAuthorization);
        UnityWebRequest request = UnityWebRequest.Get(url+uri);
        request.chunkedTransfer = false;
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("AUTHORIZATION", "Basic " + authorization);
        if(data)
        {
            request.SetRequestHeader("Accept", "multipart/related");
        }
        return request;
        /*while(!webop.isDone)
        {
        }
        // Show results as text
        String text = System.Text.Encoding.ASCII.GetString(((DownloadHandlerBuffer)webop.webRequest.downloadHandler).data);
        Debug.Log(text);
        return text;*/
    }

    public override void LoadObject(string id, string name,Transform parent,float scale,Vector3 offset)
    {
        //string text = GetRequest("/" + id + "/topic_/file");
        StartCoroutine(DownloadFile(id, name, parent, scale, offset));
    }

    public override void LoadAnnotations(string id, string name, Transform parent, float scale, Vector3 offset)
    {
        StartCoroutine(DownloadAnnotations(id, name, offset, scale));
    }

    public override void GetProjectList()
    {
        StartCoroutine(DownloadProjectList());
    }

    //Download a ModelFile from the CouchDB specified by id and name
    private IEnumerator DownloadFile(string id, string name, Transform parent,float scale,Vector3 offset)
    {
        //string text = GetRequest("/" + id + "/topic_/file");

        Debug.Log("/" + id + "/topic_/file/");
        UnityWebRequest webop = CreateGetRequest("/" + id + "/topic_/file", true);

        yield return webop.SendWebRequest();
        if (webop.isNetworkError && webop.responseCode != 200L)
        {
            Debug.Log(webop.error);
        }
        else
        {
            string text = webop.downloadHandler.text;
            Debug.Log(text.Length);
            ObjImporter importer = new ObjImporter();
            GameObject obj = new GameObject();
            obj.transform.parent = parent;
            obj.transform.localScale = new Vector3(scale, scale, scale);
            obj.name = name;
            obj.layer = 0;
            obj.AddComponent<MeshRenderer>();
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshCollider>();
            obj.GetComponent<MeshFilter>().mesh = importer.ImportFile(text);
            obj.GetComponent<MeshFilter>().sharedMesh = obj.GetComponent<MeshFilter>().mesh;
            obj.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            obj.GetComponent<MeshFilter>().mesh.RecalculateTangents();
            obj.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            obj.transform.position = offset + new Vector3(0.0f, scale*obj.GetComponent<MeshFilter>().mesh.bounds.extents.y, 0.0f);

            obj.GetComponent<MeshCollider>().sharedMesh = null;
            obj.GetComponent<MeshCollider>().sharedMesh = obj.GetComponent<MeshFilter>().mesh;
            obj.GetComponent<MeshCollider>().convex = false;
            obj.GetComponent<MeshCollider>().enabled = true;
            obj.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/defaultMat");
            obj.GetComponent<Renderer>().enabled = true;
            obj.SetActive(true);
            //Notify that Loading of Object was Successfull
            ObjectLoaded(new KeyValuePair<string, GameObject>(name,obj));
        } 
    }

    //Download Annotations for an object in the CouchDB specified by ID and name
    private IEnumerator DownloadAnnotations(string id,string name,Vector3 offset,float scale)
    {
        UnityWebRequest webop = CreateGetRequest("/" + id + "/_all_docs?include_docs=true");

        yield return webop.SendWebRequest();
        if (webop.isNetworkError && webop.responseCode != 200L)
        {
            Debug.Log(webop.error);
        }
        else
        {
            string text = webop.downloadHandler.text;
            AnnotationResponseJson resp = JsonUtility.FromJson<AnnotationResponseJson>(text);
            Dictionary<String, Annotation> annotations = new Dictionary<String, Annotation>();
            foreach (AnnotationReponseValueJson r in resp.rows)
            {
                if (r.doc._id != "info" && r.doc._id != "topic_")
                {
                    annotations.Add(r.doc._id, r.doc);
                }
            }
            //Notify, that loading of Annotations was successfull
            AnnotationsLoaded(new KeyValuePair<string, Dictionary<string, Annotation>>(name,annotations));
        }
    }

    //Download the List of all available objects in the CouchDB
    private IEnumerator DownloadProjectList()
    {
        UnityWebRequest webop = CreateGetRequest("/info/projectsInfo");
        yield return webop.SendWebRequest();
        if(webop.isNetworkError && webop.responseCode!=200L)
        {
            Debug.Log(webop.error);
        }
        else
        {
            LinkedList<Project> projectList = new LinkedList<Project>();
            string text = webop.downloadHandler.text;
            projects = JsonUtility.FromJson<ProjectInfoJson>(text);
            foreach(ProjectJson p in projects.projects)
            {
                Project newProject = new Project();
                newProject.id = p._id;
                newProject.name = p.name;
                newProject.provider = this;
                projectList.AddFirst(newProject);
            }
            //Notify that Loading a List of all available Models was successfull
            ProjectListLoaded(projectList);
        }
    }

    public override void InitService()
    {

    }

    // Update is called once per frame
    public override void UpdateService () {
		
	}
}
