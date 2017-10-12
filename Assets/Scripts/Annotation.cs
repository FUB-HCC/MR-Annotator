using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace arannotate
{
    [System.Serializable]
    public class Annotation {
        public string _id;
        public string _rev;
        public string type;
        public string status;
        public string motivation;
        public string[] responses;
        public string[] referedBy;
        public string[] referingTo;
        public string parentProject;
        public string parentTopic;
        public string creator;
        public string creationDate;
        public string created;
        public Vector3 worldCameraPosition;
        public Vector3 localCameraPosition;
        public Vector3 cameraUp;
        public string description;
        public Vector3 worldPosition;
        public Vector3 localPosition;
        public Vector3[] polygon;
        public string modified;
    }
}