using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IDataService : MonoBehaviour{
    public GameObject annotationInfoBox;
    public Action<KeyValuePair<string,GameObject>> ObjectLoaded;
    public Action<KeyValuePair<string,Dictionary<string,Annotation>>> AnnotationsLoaded;
    public Action<LinkedList<Project>> ProjectListLoaded;

    void Start()
    {
        InitService();
    }

    void Update()
    {
        UpdateService();
    }

    public abstract void InitService();
    public abstract void UpdateService();
    public abstract void GetProjectList();
    public abstract void LoadObject(string id, string name, Transform parent, float scale, Vector3 offset);
    public abstract void LoadAnnotations(string id, string name, Transform parent, float scale, Vector3 offset);
}
