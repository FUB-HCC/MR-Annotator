using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpeechManager : MonoBehaviour {
    private KeywordRecognizer keywordRecognizer = null;
    private DictationRecognizer dictationRecognizer = null;
    String dictationResult = "";
    String annotationId = "";
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();
	// Use this for initialization
	void Start () {
        GameObject parent = GameObject.Find("Holograms");
        ProjectInfoJson projects = parent.GetComponent<CouchDBWrapper>().GetProjectList();
        foreach (ProjectJson p in projects.projects)
        {
            Debug.Log(p.name);
            keywords.Add("Spawn " + p.name,()=> 
            {
                parent.GetComponent<GameObjectManager>().SpawnObject(p.name);
            });
            keywords.Add("Destroy " + p.name, () =>
            {
                parent.GetComponent<GameObjectManager>().DeleteObject(p.name);
            });
        }

        keywords.Add("Scale Object",()=> 
        {
            Debug.Log("Scale Mode");
            parent.GetComponent<GestureManager>().mode = GestureManager.ManipulationMode.MODE_SCALE;
        });
        keywords.Add("Rotate Object", () => 
        {
            Debug.Log("Rotate Mode");
            parent.GetComponent<GestureManager>().mode = GestureManager.ManipulationMode.MODE_ROTATE;
        });
        keywords.Add("Move Object", () => 
        {
            Debug.Log("Move Mode");
            parent.GetComponentInChildren<WorldCursor>().Mode = WorldCursor.ManipulationMode.MODE_MOVE;
        });
        keywords.Add("Place Object", () => 
        {
            Debug.Log("Look Mode"); parent.GetComponentInChildren<WorldCursor>().Mode = WorldCursor.ManipulationMode.MODE_LOOK;
        });
        keywords.Add("Destroy Object",() => 
        {
            parent.GetComponent<GameObjectManager>().DeleteObject(parent.GetComponentInChildren<WorldCursor>().ActiveSelection.name);
        });
        keywords.Add("Hide Annotations", () =>
        {
            parent.GetComponent<GameObjectManager>().hideAnnotations(parent.GetComponentInChildren<WorldCursor>().ActiveSelection.name,true);
        });
        keywords.Add("Show Annotations", () =>
        {
            parent.GetComponent<GameObjectManager>().hideAnnotations(parent.GetComponentInChildren<WorldCursor>().ActiveSelection.name,false);
        });
        keywords.Add("Create Annotation", () =>
        {
            Debug.Log("Create Annotation");
            keywordRecognizer.Stop();
            PhraseRecognitionSystem.Shutdown();

            dictationResult = "";

            Vector3 pos = parent.GetComponentInChildren<WorldCursor>().lookAtPoint;
            String name = parent.GetComponentInChildren<WorldCursor>().ActiveSelection.name;
            annotationId = parent.GetComponent<GameObjectManager>().createAnnotation(name, pos, dictationResult);

            dictationRecognizer.DictationResult += (text, confidence) =>
            {
                dictationResult += text;
                dictationRecognizer.Stop();
                parent.GetComponent<GameObjectManager>().updateAnnotation(name, annotationId, dictationResult);
                Debug.Log("Result: " + text);
            };
            dictationRecognizer.DictationHypothesis += (text) =>
            {
                parent.GetComponent<GameObjectManager>().updateAnnotation(name,annotationId,text);
                Debug.Log("Hypothesis: "+text);
            };
            dictationRecognizer.DictationComplete += (cause) =>
            {
                Debug.Log("Completed Dictation");
                PhraseRecognitionSystem.Restart();
                keywordRecognizer.Start();
            };
            dictationRecognizer.DictationError += (error, hresult) =>
            {

                Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
            };
            dictationRecognizer.Start();

        });

        keywords.Add("Update Annotation", () =>
        {
            GameObject selection = parent.GetComponentInChildren<WorldCursor>().ActiveSelection;
            if (selection.name == "Anchor")
            {
                Debug.Log("Update Annotation");
                keywordRecognizer.Stop();
                PhraseRecognitionSystem.Shutdown();

                dictationResult = "";

                annotationId = selection.transform.parent.name;

                dictationRecognizer.DictationResult += (text, confidence) =>
                {
                    dictationResult += text;
                    dictationRecognizer.Stop();
                    String objName = selection.transform.parent.parent.parent.name;
                    parent.GetComponent<GameObjectManager>().updateAnnotation(objName, annotationId, text);
                    Debug.Log("Result: " + text);
                };
                dictationRecognizer.DictationHypothesis += (text) =>
                {
                    String objName = selection.transform.parent.parent.parent.name;
                    parent.GetComponent<GameObjectManager>().updateAnnotation(objName, annotationId, text);
                    Debug.Log("Hypothesis: " + text);
                };
                dictationRecognizer.DictationComplete += (cause) =>
                {
                    Debug.Log("Completed Dictation");
                    PhraseRecognitionSystem.Restart();
                    keywordRecognizer.Start();
                };
                dictationRecognizer.DictationError += (error, hresult) =>
                {

                    Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
                };
                dictationRecognizer.Start();
            }
        });

        keywords.Add("Delete Annotation", () =>
        {
            GameObject selection = parent.GetComponentInChildren<WorldCursor>().ActiveSelection;
            if(selection.name=="Anchor")
            {
                Debug.Log("Delete Annotation");
                parent.GetComponent<GameObjectManager>().DeleteAnnotation(selection);
            }
        });

        dictationRecognizer = new DictationRecognizer(ConfidenceLevel.Medium);
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += onKeywordRecognized;
        keywordRecognizer.Start();
	}

    private void onKeywordRecognized(PhraseRecognizedEventArgs args)
    {
        Action action;
        if (keywords.TryGetValue(args.text,out action))
        {
            action.Invoke();
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
