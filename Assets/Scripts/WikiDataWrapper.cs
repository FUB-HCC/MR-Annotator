using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Web;
using System.Xml;

public class WikiDataWrapper : IDataService {

    private UnityWebRequest CreateGetRequest(string query, bool data = false)
    {
        UnityWebRequest request = UnityWebRequest.Get("https://query.wikidata.org/sparql?query=" + query);
        request.chunkedTransfer = false;
        request.downloadHandler = new DownloadHandlerBuffer();
        if (data)
        {
            request.SetRequestHeader("Accept", "multipart/related");
        }
        return request;
    }

    public override void GetProjectList()
    {
        StartCoroutine(DownloadProjectList());
    }

    public override void LoadObject(string id, string name, Transform parent, float scale, Vector3 offset)
    {
        StartCoroutine(DownloadFile(id, name, parent, scale, offset));
    }

    public override void LoadAnnotations(string id, string name, Transform parent, float scale, Vector3 offset)
    {
        StartCoroutine(DownloadAnnotations(id, name, offset, scale));
    }

    private IEnumerator DownloadFile(string id, string name, Transform parent, float scale, Vector3 offset)
    {
        string[] objId = id.Split('/');
        string query = @"SELECT ?model ?modelLabel
                            WHERE
                            {
                                wd:" + objId[objId.Length-1] + @" wdt:P4896 ?model.
                                SERVICE wikibase:label { bd:serviceParam wikibase:language 'en'.}
                            }";

        UnityWebRequest webop = CreateGetRequest(query, false);

        yield return webop.SendWebRequest();
        if (webop.isNetworkError && webop.responseCode != 200L)
        {
            Debug.Log(webop.error);
        }
        else
        {
            string text = webop.downloadHandler.text;
            Debug.Log(text.Length);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);
            XmlNodeList results = doc.GetElementsByTagName("result");
            string url = "";
            foreach (XmlNode node in results)
            {
                XmlNodeList bindings = node.ChildNodes;
                foreach (XmlNode binding in bindings)
                {
                    XmlAttributeCollection attrs = binding.Attributes;
                    foreach (XmlAttribute attr in attrs)
                    {
                        if (attr.Name == "name" && attr.Value == "model")
                        {
                            url = binding.FirstChild.FirstChild.Value;
                        }
                    }
                }
            }

            StartCoroutine(DownloadObject(url,name,parent,scale,offset));
        }
    }

    private IEnumerator DownloadObject(string url, string name, Transform parent, float scale, Vector3 offset)
    {
        UnityWebRequest webop = UnityWebRequest.Get(url);
        webop.chunkedTransfer = false;
        webop.SetRequestHeader("Accept", "multipart/related");
        webop.downloadHandler = new DownloadHandlerBuffer();
        yield return webop.SendWebRequest();
        if (webop.isNetworkError && webop.responseCode != 200L)
        {
            Debug.Log(webop.error);
        }
        else
        {
            byte[] text = webop.downloadHandler.data;
            StlImporter importer = new StlImporter();
            GameObject obj = new GameObject();
            obj.transform.parent = parent;
            obj.transform.localScale = new Vector3(scale, scale, scale);
            obj.transform.position = offset;
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
            obj.GetComponent<MeshCollider>().sharedMesh = null;
            obj.GetComponent<MeshCollider>().sharedMesh = obj.GetComponent<MeshFilter>().mesh;
            obj.GetComponent<MeshCollider>().convex = false;
            obj.GetComponent<MeshCollider>().enabled = true;
            obj.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/defaultMat");
            obj.GetComponent<Renderer>().enabled = true;
            obj.SetActive(true);
            ObjectLoaded(new KeyValuePair<string, GameObject>(name, obj));
        }
    }

    private IEnumerator DownloadAnnotations(string id, string name, Vector3 offset, float scale)
    {
        string[] objId = id.Split('/');
        string query = @"SELECT ?wdLabel ?ps_Label ?wdpqLabel ?pq_Label
                         {
                             VALUES (?object) {(wd:"+objId[objId.Length-1]+@")}
                             ?object ?p ?statement.
                             ?statement ?ps ?ps_.
                             ?wd wikibase:claim ?p.
                             ?wd wikibase:statementProperty ?ps.
                             OPTIONAL
                             {
                                ?statement ?pq ?pq_.
                                ?wdpq wikibase:qualifier ?pq.
                             }
                             SERVICE wikibase:label { bd:serviceParam wikibase:language 'en'}
                         }";
        UnityWebRequest webop = CreateGetRequest(query);

        yield return webop.SendWebRequest();
        if (webop.isNetworkError && webop.responseCode != 200L)
        {
            Debug.Log(webop.error);
        }
        else
        {
            string text = webop.downloadHandler.text;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);
            Dictionary<String, Annotation> annotations = new Dictionary<String, Annotation>();
            XmlNodeList results = doc.GetElementsByTagName("result");
            foreach (XmlNode node in results)
            {
                Annotation annotation = new Annotation();
                annotation.creationDate = DateTime.Today.ToString();
                annotation.localPosition = UnityEngine.Random.onUnitSphere * 200;
                annotation.localPosition.y = Math.Abs(annotation.localPosition.y);
                XmlNodeList bindings = node.ChildNodes;
                foreach (XmlNode binding in bindings)
                {
                    XmlAttributeCollection attrs = binding.Attributes;
                    foreach (XmlAttribute attr in attrs)
                    {
                        if (attr.Name == "name" && attr.Value == "wdLabel")
                        {
                            annotation._id = binding.FirstChild.FirstChild.Value;
                        }
                        else if (attr.Name == "name" && attr.Value == "ps_Label")
                        {
                            annotation.description = binding.FirstChild.FirstChild.Value;
                        }
                    }
                }
                if(!annotations.ContainsKey(annotation._id))
                { 
                    annotations.Add(annotation._id, annotation);
                }
            }
            //Notify, that loading of Annotations was successfull
            AnnotationsLoaded(new KeyValuePair<string, Dictionary<string, Annotation>>(name, annotations));
        }
    }

    public IEnumerator DownloadProjectList()
    {
        string query = @"SELECT ?object ?objectLabel ?modelLabel
                         WHERE
                         {
                             ?object wdt:P4896 ?model.
                             SERVICE wikibase:label { bd:serviceParam wikibase:language 'en'. }
                         }";
        UnityWebRequest webop = CreateGetRequest(query,true);
        yield return webop.SendWebRequest();
        if (webop.isNetworkError && webop.responseCode != 200L)
        {
            Debug.Log(webop.error);
        }
        else
        {
            string text = webop.downloadHandler.text;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);
            LinkedList<Project> projectList = new LinkedList<Project>();
            XmlNodeList results = doc.GetElementsByTagName("result");
            foreach(XmlNode node in results)
            {
                Project newProject = new Project();
                XmlNodeList bindings = node.ChildNodes;
                foreach(XmlNode binding in bindings)
                {
                    XmlAttributeCollection attrs = binding.Attributes;
                    foreach(XmlAttribute attr in attrs)
                    {
                        if(attr.Name == "name" && attr.Value == "object")
                        {
                            newProject.id = binding.FirstChild.FirstChild.Value;
                        }
                        else if(attr.Name == "name" && attr.Value == "objectLabel")
                        {
                            newProject.name = binding.FirstChild.FirstChild.Value;
                        }
                    }
                }
                projectList.AddFirst(newProject);
            }
            //Notify that Loading a List of all available Models was successfull
            ProjectListLoaded(projectList);
        }
    }

    // Use this for initialization
    public override void InitService () {
		
	}
	
	// Update is called once per frame
	public override void UpdateService () {
		
	}
}
