/*==============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.

Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using System;
using UnityEngine;
using Vuforia;

/// <summary>
///     A custom handler that implements the ITrackableEventHandler interface.
/// </summary>
public class DefaultTrackableEventHandler : MonoBehaviour, ITrackableEventHandler
{
    #region PRIVATE_MEMBER_VARIABLES

    protected TrackableBehaviour mTrackableBehaviour;

    #endregion // PRIVATE_MEMBER_VARIABLES

    #region UNTIY_MONOBEHAVIOUR_METHODS

    protected virtual void Start()
    {
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS

    #region PUBLIC_METHODS

    /// <summary>
    ///     Implementation of the ITrackableEventHandler function called when the
    ///     tracking state changes.
    /// </summary>
    public void OnTrackableStateChanged(
        TrackableBehaviour.Status previousStatus,
        TrackableBehaviour.Status newStatus)
    {
        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED ||
            newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            string[] marker = mTrackableBehaviour.TrackableName.Split('-');
            GameObject nMarkBase = GameObject.Find("MarkerBases/" + marker[0]);
            //GameObject nMarkBase = GameObject.Find("MarkerBases/" + "shuttle");
            if (nMarkBase==null)
            {
                GameObject markBases = GameObject.Find("MarkerBases");
                nMarkBase = new GameObject(marker[0]);
                //nMarkBase = new GameObject("shuttle");
                nMarkBase.transform.SetParent(markBases.transform);
                nMarkBase.AddComponent<MarkerAugmentation>();
            }
            MarkerAugmentation markaug = nMarkBase.GetComponent<MarkerAugmentation>();
            GameObjectManager goManager = GameObject.Find("Holograms").GetComponent<GameObjectManager>();

            markaug.trackerState[Int32.Parse(marker[1]), Int32.Parse(marker[2])] = true;
            markaug.trackerPos[Int32.Parse(marker[1]), Int32.Parse(marker[2])] = mTrackableBehaviour.transform.position;
            //markaug.trackedObject = "shuttle";
            markaug.trackedObject = marker[0];

            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " found");
            OnTrackingFound();
        }
        else if (previousStatus == TrackableBehaviour.Status.TRACKED &&
                 newStatus == TrackableBehaviour.Status.NOT_FOUND)
        {
            string[] marker = mTrackableBehaviour.TrackableName.Split('-');
            GameObject nMarkBase = GameObject.Find("MarkerBases/" + marker[0]);
            //GameObject nMarkBase = GameObject.Find("MarkerBases/" + "shuttle");

            if (nMarkBase == null)
            {
                GameObject markBases = GameObject.Find("MarkerBases");
                nMarkBase = new GameObject(marker[0]);
                //nMarkBase = new GameObject("shuttle");
                nMarkBase.transform.SetParent(markBases.transform);
                nMarkBase.AddComponent<MarkerAugmentation>();
            }
            MarkerAugmentation markaug = nMarkBase.GetComponent<MarkerAugmentation>();
            markaug.trackerState[Int32.Parse(marker[1]), Int32.Parse(marker[2])] = false;
            markaug.trackerPos[Int32.Parse(marker[1]), Int32.Parse(marker[2])] = mTrackableBehaviour.transform.position;
            markaug.trackedObject = marker[0];
            //markaug.trackedObject = "shuttle";
            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");
            OnTrackingLost();
        }
        else
        {
            // For combo of previousStatus=UNKNOWN + newStatus=UNKNOWN|NOT_FOUND
            // Vuforia is starting, but tracking has not been lost or found yet
            // Call OnTrackingLost() to hide the augmentations
            MarkerAugmentation markaug = GetComponentInParent<MarkerAugmentation>();
            string[] marker = mTrackableBehaviour.TrackableName.Split('-');
            markaug.trackerState[Int32.Parse(marker[1]), Int32.Parse(marker[2])] = false;
            markaug.trackerPos[Int32.Parse(marker[1]), Int32.Parse(marker[2])] = mTrackableBehaviour.transform.position;
            markaug.trackedObject = marker[0];
            //markaug.trackedObject = "shuttle";
            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");
            OnTrackingLost();
        }
    }

    #endregion // PUBLIC_METHODS

    #region PRIVATE_METHODS

    protected virtual void OnTrackingFound()
    {
        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);
        var canvasComponents = GetComponentsInChildren<Canvas>(true);

        // Enable rendering:
        foreach (var component in rendererComponents)
        {
            component.enabled = true;
        }
        // Enable colliders:
        foreach (var component in colliderComponents)
            component.enabled = true;

        // Enable canvas':
        foreach (var component in canvasComponents)
            component.enabled = true;
    }


    protected virtual void OnTrackingLost()
    {
        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);
        var canvasComponents = GetComponentsInChildren<Canvas>(true);

        // Disable rendering:
        foreach (var component in rendererComponents)
            component.enabled = false;

        // Disable colliders:
        foreach (var component in colliderComponents)
            component.enabled = false;

        // Disable canvas':
        foreach (var component in canvasComponents)
            component.enabled = false;
    }

    #endregion // PRIVATE_METHODS
}
