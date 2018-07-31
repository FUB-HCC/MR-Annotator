using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Web;
using System.Xml;

public class DBPediaWrapper : IDataService {

    private UnityWebRequest CreateGetRequest(string query, bool data = false)
    {
        UnityWebRequest request = UnityWebRequest.Get("https://dbpedia.org/sparql?default-graph-uri=http://dbpedia.org&query=" + query);
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
    }

    public override void LoadObject(string id, string name, Transform parent, float scale, Vector3 offset)
    {
    }

    public override void LoadAnnotations(string id, string name, Transform parent, float scale, Vector3 offset)
    {
        StartCoroutine(DownloadAnnotations(id, name, offset, scale));
    }


    private IEnumerator DownloadAnnotations(string id, string name, Vector3 offset, float scale)
    {
        string[] objId = id.Split('/');
        string query = @"SELECT ?objectLabel ?label ?value
                         {
                             VALUES (?object) {(dbr:"+objId[objId.Length-1]+@")}
                             ?object ?p ?value.
                             ?object rdfs:label ?objectLabel.
                             ?p rdfs:label ?label.
                             FILTER (LANG(?objectLabel) = 'en')
                             FILTER(LANG(?label) = 'en')
                             FILTER(LANG(?value) = 'en' || LANG(?value) = '')
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
                        if (attr.Name == "name" && attr.Value == "label")
                        {
                            annotation._id = binding.FirstChild.FirstChild.Value;
                        }
                        else if (attr.Name == "name" && attr.Value == "value")
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

    // Use this for initialization
    public override void InitService () {
		
	}
	
	// Update is called once per frame
	public override void UpdateService () {
		
	}
}
