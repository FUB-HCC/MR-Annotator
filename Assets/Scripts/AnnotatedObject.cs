using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnnotatedObject {
    public GameObject annotatedObject;
    public Dictionary<String,Annotation> annotations = new Dictionary<String,Annotation>();
    public bool showAnnotations;

    public AnnotatedObject(GameObject obj,Dictionary<String,Annotation> annotations,GameObject annotationInfoBox)
    {
        this.annotatedObject = obj;
        this.annotations = annotations;

        GameObject annotationContainer = new GameObject("Annotations");
        annotationContainer.transform.parent = annotatedObject.transform;

        //Create Anchors on Objects from existing Annotations
        List<Vector3> anchorVerts = new List<Vector3>();
        List<int> anchorIdx = new List<int>();
        List<Color> colors = new List<Color>();
        foreach(Annotation a in annotations.Values)
        {
            GameObject cont = new GameObject();
            cont.name = a._id;
            cont.transform.parent = annotationContainer.transform;
            GameObject annotationAnchor = new GameObject();
            TextMesh annotationText = annotationAnchor.AddComponent<TextMesh>();

            //Create GUI
            Canvas annotationCanvas = cont.AddComponent<Canvas>();
            annotationCanvas.worldCamera = Camera.main;
            annotationCanvas.name = "Canvas";
            annotationCanvas.renderMode = RenderMode.WorldSpace;
            annotationCanvas.transform.parent = cont.transform;
            annotationCanvas.transform.position = a.localPosition;
            annotationCanvas.enabled = true;

            Text annotationLabel = cont.AddComponent<Text>();
            annotationLabel.name = "Text";
            annotationLabel.transform.parent = cont.transform;
            annotationLabel.text = a.description;
            annotationLabel.color = Color.blue;
            annotationLabel.enabled = true;

            annotationText.name = "Text";
            annotationText.transform.parent = cont.transform;
            annotationText.text = /*a.description*/"";
            annotationText.color = Color.red;
            annotationText.transform.position = a.localPosition;
            annotationText.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            annotationAnchor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            annotationAnchor.name = "Anchor";
            annotationAnchor.transform.parent = cont.transform;
            annotationAnchor.transform.position = a.localPosition;
            annotationAnchor.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            anchorVerts.Add(a.localPosition);
        }
    }

    public void deleteAnnotation(String id)
    {
        annotations.Remove(id);
    }

    public void updateAnnotation(String id,String text)
    {
        Debug.Log("Annotated Object");
        annotations[id].description = text;
        GameObject annotationText = annotatedObject.transform.Find("Annotations/" + id + "/Text").gameObject;
        annotationText.GetComponent<TextMesh>().text = text;
    }

    public String addAnnotation(Vector3 pos,String text)
    {
        List<Vector3> anchorVerts = new List<Vector3>();
        List<int> anchorIdx = new List<int>();
        List<Color> colors = new List<Color>();
        //TODO Fill Annotation Structure
        Annotation annotation = new Annotation();
        annotation._id = System.Guid.NewGuid().ToString();
        annotation.created = DateTime.Now.ToUniversalTime().ToString();
        annotation.creationDate = annotation.created;
        annotation.description = text;
        annotation.localPosition = pos;
        annotations.Add(annotation._id, annotation);

        GameObject container = annotatedObject.transform.Find("Annotations").gameObject;
        GameObject cont = new GameObject();
        cont.transform.parent = container.transform;
        cont.name = annotation._id;
        GameObject annotationAnchor = new GameObject();
        //Create GUI
        Canvas annotationCanvas = cont.AddComponent<Canvas>();
        annotationCanvas.worldCamera = Camera.main;
        annotationCanvas.name = "Canvas";
        annotationCanvas.renderMode = RenderMode.WorldSpace;
        annotationCanvas.transform.parent = cont.transform;
        annotationCanvas.transform.position = pos;
        annotationCanvas.enabled = true;

        Text annotationLabel = cont.AddComponent<Text>();
        annotationLabel.name = "Text";
        annotationLabel.transform.parent = cont.transform;
        annotationLabel.text = text;
        annotationLabel.color = Color.blue;
        annotationLabel.enabled = true;

        //TextMesh annotationText = cont.AddComponent<TextMesh>();
        //annotationText.name = "Text";
        //annotationText.transform.parent = cont.transform;
        //annotationText.text = /*text*/"";
        //annotationText.color = Color.red;
        //annotationText.transform.position = pos;
        //annotationText.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //annotationAnchor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //annotationAnchor.name = "Anchor";
        //annotationAnchor.transform.parent = cont.transform;
        //annotationAnchor.transform.position = pos;
        //annotationAnchor.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        return annotation._id;
    }

    public void hideAnnotations(bool hide)
    {
        annotatedObject.transform.Find("Annotations").gameObject.SetActive(!hide);
    }
}
