using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Class representing an Annotated Object. Holds the virtual representation of a physical object in a Gameobject.
 * A Dictonary of pairs of annotation ids and Annotations hold the actual Annotations
 */
public class AnnotatedObject {
    public string name;
    public GameObject annotatedObject;
    public Dictionary<String,Annotation> annotations = new Dictionary<String,Annotation>();
    public bool showAnnotations;

    //Create an empty Annotated Object without mesh and Annotations but with a name to identify
    public AnnotatedObject(String name)
    {
        this.name = name;
        this.annotatedObject = null;
        this.annotations = null;
    }

    //Create an Annotated Object from a GameObject that holds the virutal representation of a physical Object,
    //a Dictonary of Annotations and a AnnotationInfoBox to visualize the actual annotations.
    public AnnotatedObject(GameObject obj,Dictionary<String,Annotation> annotations,GameObject annotationInfoBoxPrefab)
    {
        this.annotatedObject = obj;
        this.annotations = annotations;

        GameObject annotationContainer = new GameObject("Annotations");

        annotationContainer.transform.parent = annotatedObject.transform;

        //Create Anchors on Objects from existing Annotations
        foreach(Annotation a in annotations.Values)
        {
            GameObject cont = new GameObject();
            cont.name = a._id;
            cont.transform.parent = annotationContainer.transform;
            cont.transform.position = a.localPosition;

            //Create GUI
            GameObject annotationInfoBox = GameObject.Instantiate(annotationInfoBoxPrefab);
            annotationInfoBox.SetActive(true);
            annotationInfoBox.name = "AnnotationBox";
            annotationInfoBox.GetComponent<Canvas>().worldCamera = Camera.main;
            annotationInfoBox.GetComponent<RectTransform>().parent = cont.transform;
            annotationInfoBox.GetComponent<RectTransform>().localScale = new Vector3(0.0002f, 0.0002f, 0.0002f);
            Vector3 dir = (0.01f * a.localPosition - 0.01f*annotatedObject.GetComponent<MeshFilter>().mesh.bounds.center).normalized;
            //Just create Annotationboxes above the actual annotation Anchor
            if(dir.y<0.0f)
            {
                dir.y = 0.3f;
                dir = dir.normalized;
            }
            annotationInfoBox.GetComponent<RectTransform>().position = annotatedObject.transform.position+(0.01f*a.localCameraPosition + 0.01f*1.5f*annotatedObject.GetComponent<MeshFilter>().mesh.bounds.extents.magnitude*dir);

            annotationInfoBox.GetComponent<AnnotationBoxBehaviour>().annObj = this;
            annotationInfoBox.GetComponent<AnnotationBoxBehaviour>().SetPosition(a.localPosition);
            annotationInfoBox.GetComponent<AnnotationBoxBehaviour>().SetHeader(a.creationDate);
            annotationInfoBox.GetComponent<AnnotationBoxBehaviour>().SetContent(a.description);
        }
        annotationContainer.transform.parent.GetComponent<MeshRenderer>().enabled = true;
    }

    public void setObject(GameObject obj)
    {
        this.annotatedObject = obj;
        GameObject annotationContainer = new GameObject("Annotations");
        annotationContainer.transform.parent = annotatedObject.transform;
    }

    public void addAnnotations(Dictionary<string,Annotation> annotations, GameObject annotationInfoBoxPrefab)
    {
        this.annotations = annotations;
        GameObject annotationContainer = GameObject.Find(annotatedObject.name+"/Annotations");
        annotationContainer.transform.parent = annotatedObject.transform;

        //Create Anchors on Objects from existing Annotations
        foreach (Annotation a in annotations.Values)
        {
            GameObject cont = new GameObject();
            cont.name = a._id;
            cont.transform.parent = annotationContainer.transform;
            cont.transform.position = a.localPosition;

            //Create GUI
            GameObject annotationInfoBox = GameObject.Instantiate(annotationInfoBoxPrefab);
            annotationInfoBox.SetActive(true);
            annotationInfoBox.name = "AnnotationBox";
            annotationInfoBox.GetComponent<Canvas>().worldCamera = Camera.main;
            annotationInfoBox.GetComponent<RectTransform>().parent = cont.transform;
            annotationInfoBox.GetComponent<RectTransform>().localScale = new Vector3(0.0002f, 0.0002f, 0.0002f);
            Vector3 dir = (0.01f * a.localPosition - 0.01f * annotatedObject.GetComponent<MeshFilter>().mesh.bounds.center).normalized;
            //Just create Annotationboxes above the actual annotation Anchor
            if (dir.y < 0.0f)
            {
                dir.y = 0.3f;
                dir = dir.normalized;
            }
            annotationInfoBox.GetComponent<RectTransform>().position = annotatedObject.transform.position + (0.001f * a.localPosition + 0.001f * 1.5f * annotatedObject.GetComponent<MeshFilter>().mesh.bounds.extents.magnitude * dir);

            annotationInfoBox.GetComponent<AnnotationBoxBehaviour>().annObj = this;
            annotationInfoBox.GetComponent<AnnotationBoxBehaviour>().SetPosition(a.localPosition);
            annotationInfoBox.GetComponent<AnnotationBoxBehaviour>().SetHeader(a._id);
            annotationInfoBox.GetComponent<AnnotationBoxBehaviour>().SetContent(a.description);
        }
        annotationContainer.transform.parent.GetComponent<MeshRenderer>().enabled = true;
    }

    public void deleteAnnotation(String id)
    {
        annotations.Remove(id);
    }

    public void updateAnnotation(String id,String text)
    {
        Debug.Log("Annotated Object");
        annotations[id].description = text;
        annotatedObject.transform.Find("Annotations/" + id + "/AnnotationBox").gameObject.GetComponent<AnnotationBoxBehaviour>().SetContent(text);
    }

    public String addAnnotation(Vector3 pos,String text, GameObject annotationInfoBoxPrefab)
    {
        //TODO Fill Annotation Structure
        Annotation annotation = new Annotation();
        annotation._id = System.Guid.NewGuid().ToString();
        annotation.created = DateTime.Now.ToUniversalTime().ToString();
        annotation.creationDate = annotation.created;
        annotation.description = text;
        annotation.localPosition = pos - annotatedObject.transform.position;
        annotations.Add(annotation._id, annotation);

        GameObject annotationContainer = GameObject.Find(annotatedObject.name + "/Annotations");
        annotationContainer.transform.parent = annotatedObject.transform;

        GameObject cont = new GameObject();
        cont.name = annotation._id;
        cont.transform.parent = annotationContainer.transform;
        cont.transform.position = annotation.localPosition;

        //Create GUI
        GameObject annotationInfoBox = GameObject.Instantiate(annotationInfoBoxPrefab);
        annotationInfoBox.SetActive(true);
        annotationInfoBox.name = "AnnotationBox";
        annotationInfoBox.GetComponent<Canvas>().worldCamera = Camera.main;
        annotationInfoBox.GetComponent<RectTransform>().parent = cont.transform;
        annotationInfoBox.GetComponent<RectTransform>().localScale = new Vector3(0.0002f, 0.0002f, 0.0002f);
        Vector3 dir = (0.01f * annotation.localPosition - 0.01f * annotatedObject.GetComponent<MeshFilter>().mesh.bounds.center).normalized;
        //Just create Annotationboxes above the actual annotation Anchor
        if (dir.y < 0.0f)
        {
            dir.y = 0.3f;
            dir = dir.normalized;
        }
        annotationInfoBox.GetComponent<RectTransform>().position = annotatedObject.transform.position + (0.001f * annotation.localPosition + 0.001f * 1.5f * annotatedObject.GetComponent<MeshFilter>().mesh.bounds.extents.magnitude * dir);

        annotationInfoBox.GetComponent<AnnotationBoxBehaviour>().annObj = this;
        annotationInfoBox.GetComponent<AnnotationBoxBehaviour>().SetPosition(annotation.localPosition);
        annotationInfoBox.GetComponent<AnnotationBoxBehaviour>().SetHeader(annotation._id);
        annotationInfoBox.GetComponent<AnnotationBoxBehaviour>().SetContent(annotation.description);

        annotationContainer.transform.parent.GetComponent<MeshRenderer>().enabled = true;


        return annotation._id;
    }

    public void hideAnnotations(bool hide)
    {
        annotatedObject.transform.Find("Annotations").gameObject.SetActive(!hide);
    }
}
