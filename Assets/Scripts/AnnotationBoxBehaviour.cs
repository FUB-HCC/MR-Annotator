using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationBoxBehaviour : MonoBehaviour {
    private Vector3 annPos;
    public AnnotatedObject annObj;

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 annPosWorld = (0.001f * annPos);
        if (annObj != null)
        {
            annPosWorld = (0.001f * annPos) + annObj.annotatedObject.transform.position;
        }
        RaycastHit hitInfo;

        if (Physics.Raycast(cameraPos, annPosWorld - cameraPos, out hitInfo))
        {
            Vector3 forwardCamera = Camera.main.transform.forward;
            Vector3 forwardGui = -1.0f * GetComponent<RectTransform>().forward;
            Vector3 axis = -Camera.main.transform.position + GetComponent<RectTransform>().position;
            //GetComponent<RectTransform>().rotation = Camera.main.transform.rotation;
            GetComponent<RectTransform>().rotation = Quaternion.LookRotation(axis);

            //Fade Objects
            GameObject line = transform.Find("Line").gameObject;
            GameObject header = transform.Find("Header").gameObject;
            GameObject headerText = transform.Find("Header/HeaderText").gameObject;
            GameObject body = transform.Find("Content").gameObject;
            GameObject bodyText = transform.Find("Content/ContentText").gameObject;
            Color c = header.GetComponent<Image>().color;
            c.a = 0.3f/Vector3.Distance(cameraPos, annPosWorld);
            header.GetComponent<Image>().color = c;
            c = body.GetComponent<Image>().color;
            c.a = 0.3f / Vector3.Distance(cameraPos, annPosWorld);
            body.GetComponent<Image>().color = c;
            c = headerText.GetComponent<Text>().color;
            c.a = 0.3f / Vector3.Distance(cameraPos, annPosWorld);
            headerText.GetComponent<Text>().color = c;
            c = bodyText.GetComponent<Text>().color;
            c.a = 0.3f / Vector3.Distance(cameraPos, annPosWorld);
            bodyText.GetComponent<Text>().color = c;

            c = line.GetComponent<LineRenderer>().startColor;
            c.a = 0.3f / Vector3.Distance(cameraPos, annPosWorld);
            c = line.GetComponent<LineRenderer>().startColor = c;

            c = line.GetComponent<LineRenderer>().endColor;
            c.a = 0.3f / Vector3.Distance(cameraPos, annPosWorld);
            c = line.GetComponent<LineRenderer>().endColor = c;

            if (Math.Abs(Vector3.Distance(cameraPos,hitInfo.point)-Vector3.Distance(cameraPos,annPosWorld))<0.0001f)
            {
                //GetComponent<RectTransform>().Rotate(axis, angle);
                GetComponent<Canvas>().enabled = true;
                transform.Find("Line").GetComponent<LineRenderer>().enabled = true;
            }
            else
            {
                //GetComponent<Canvas>().enabled = false;
                //transform.Find("Line").GetComponent<LineRenderer>().enabled = false;
            }
        }
    }

    public void SetPosition(Vector3 annPos)
    {
        this.annPos = annPos;
        transform.Find("Line").GetComponent<LineRenderer>().widthMultiplier = 0.001f;
        transform.Find("Line").GetComponent<LineRenderer>().SetPosition(0, new Vector4(annObj.annotatedObject.transform.position.x, annObj.annotatedObject.transform.position.y, annObj.annotatedObject.transform.position.z, 1.0f) + GetComponent<RectTransform>().parent.localToWorldMatrix * (0.001f*annPos));
        transform.Find("Line").GetComponent<LineRenderer>().SetPosition(1, GetComponent<RectTransform>().parent.localToWorldMatrix * GetComponent<RectTransform>().position);
        transform.Find("Line").GetComponent<LineRenderer>().enabled = true;
    }

    public void SetHeader(string text)
    {
        GameObject header = transform.Find("Header/HeaderText").gameObject;
        Text t = header.GetComponent<Text>();
        t.text = text;
    }

    public void SetContent(string text)
    {
        GameObject header = transform.Find("Content/ContentText").gameObject;
        Text t = header.GetComponent<Text>();
        t.text = text;
    }
}
