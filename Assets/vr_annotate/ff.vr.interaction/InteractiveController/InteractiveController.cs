using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VR.WSA.Input;
using System;
using ff.vr.annotate.viz;
using ff.nodegraph.interaction;

namespace ff.vr.interaction
{
    /*public interface ITeleportationTrigger
    {
        void PadClicked(Teleportation teleportation);
        void PadUnclicked(Teleportation teleportation);
    }*/

    /// <summary>
    /// This Compononent handles the interaction with Vive Controllers for teleportation, 
    /// annotations, drag-gizmos, buttons, etc.
    /// </summary>
    //[RequireComponent(typeof(SteamVR_TrackedController))]
    public class InteractiveController : MonoBehaviour
    {
        public GameObject LaserPrefab;

        // This is override by derived classes
        protected virtual void Start()
        {
            Initialize();
        }

        protected void Initialize()
        {
            if (_controllerInitialized)
                return;

            _laserInstance = Instantiate(LaserPrefab);
            _laserInstance.transform.SetParent(transform, false);
            _laserPointer = _laserInstance.GetComponent<LaserPointer>();
            _laserPointer.Controller = this;

            //_teleportation = GetComponent<Teleportation>();
            _audioSourceForTeleportation = GetComponent<AudioSource>();
            _controllerInitialized = true;
        }
        private bool _controllerInitialized = false;

        private AudioSource _audioSourceForTeleportation;

        protected virtual void OnEnable()
        {
            // Setup EventHandlers
            gestureManager = GestureManager.Instance;
            gestureManager.gestureRecognizer.TappedEvent += TriggerClickedHandler;
            gestureManager.gestureRecognizer.TappedEvent += MenuButtonClickedHandler;
            /*_controller = GetComponent<SteamVR_TrackedController>();
            if (_controller == null)
            {
                Debug.LogError("SteamVR_Teleporter must be on a SteamVR_TrackedController");
                return;
            }
            _controller.TriggerClicked += TriggerClickedHandler;
            _controller.TriggerUnclicked += TriggerUnclickedHandler;
            _controller.PadClicked += PadClickedHandler;
            _controller.PadUnclicked += PadUnclickedHandler;
            _controller.Gripped += ControllerGrippedHandler;
            _controller.MenuButtonClicked += MenuButtonClickedHandler;
            _controller.MenuButtonUnclicked += MenuButtonUnclickedHandler;

            UpdateListofGizmoColliders();*/
        }

        protected virtual void OnDisable()
        {
            gestureManager = GestureManager.Instance;
            gestureManager.gestureRecognizer.TappedEvent -= TriggerClickedHandler;
            gestureManager.gestureRecognizer.TappedEvent -= MenuButtonClickedHandler;
            /*if (_controller == null)
                return;

            _controller.TriggerClicked -= TriggerClickedHandler;
            _controller.TriggerUnclicked -= TriggerUnclickedHandler;
            _controller.PadClicked -= PadClickedHandler;
            _controller.PadUnclicked -= PadUnclickedHandler;
            _controller.Gripped -= ControllerGrippedHandler;
            _controller.MenuButtonClicked -= MenuButtonClickedHandler;
            _controller.MenuButtonUnclicked -= MenuButtonUnclickedHandler;*/
        }


        public void UpdateListofGizmoColliders()
        {
            foreach (var h in FindObjectsOfType<InteractiveGizmo>())
            {
                var collider = h.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    GizmoColliders.Add(collider);
                }
            }
        }

        public void SetLaserPointerEnabled(bool enabled)
        {
            if (_laserPointer)
                _laserPointer.SetLaserpointerEnabled(enabled);
        }

        private List<BoxCollider> GizmoColliders = new List<BoxCollider>();
        protected virtual void Update()
        {
            // Debug.Log(_state);
            // Find current hover interactive handle
            var newHoverGizmo = GetGizmoUnderController();

            if (_state == States.Default)
            {
                if (newHoverGizmo)
                {
                    newHoverGizmo.OnControllerEnter(this);
                    _state = States.IsCollidingWithGizmo;
                    _currentHoverGizmo = newHoverGizmo;
                    _laserPointer.SetLaserpointerEnabled(false);
                }
            }
            else if (_state == States.IsCollidingWithGizmo)
            {
                if (newHoverGizmo == _currentHoverGizmo)
                {
                    _currentHoverGizmo.OnControllerStay(this);
                }
                else
                {
                    _currentHoverGizmo.OnControllerExit(this);
                    _state = States.Default;
                    _currentHoverGizmo = null;
                    _laserPointer.SetLaserpointerEnabled(true);
                }
            }
            else if (_state == States.DraggingGizmo)
            {
                _currentHoverGizmo.OnDragUpdate(this);
            }
            else if (_state == States.PointerCapturedOnClickable && !(_laserPointer.PointingAt is IClickableLaserPointerTarget))
            {
                _capturedClickTarget = null;
                _state = States.Default;
                _laserPointer.IsLockedAtTarget = false;
            }
            else if (_state == States.PointerCapturedOnTeleporter /*&& !(_laserPointer.PointingAt is ITeleportationTrigger)*/)
            {
                _state = States.Default;
            }
        }

        private void TriggerClickedHandler(InteractionSourceKind source, int tapCount, Ray headRayedEventArgs)
        {
            if (tapCount == 1)
            {
                if (_laserPointer == null)
                {
                    Debug.LogError("LaserPointer component not found for interactiveController");
                    return;
                }

                if (_state == States.Default)
                {
                    if (_laserPointer.PointingAt is IClickableLaserPointerTarget)
                    {
                        _capturedClickTarget = _laserPointer.PointingAt as IClickableLaserPointerTarget;
                        _capturedClickTarget.PointerTriggered(_laserPointer);
                        _state = States.PointerCapturedOnClickable;
                        _laserPointer.IsLockedAtTarget = true;
                    }
                    else
                    {
                        // _capturedClickTarget = null;
                        NodeSelector.Instance.HandleNullHit();
                    }
                }
                else if (_state == States.IsCollidingWithGizmo)
                {
                    if (_currentHoverGizmo.OnDragStarted(this))
                        _state = States.DraggingGizmo;
                }
            }
        }

        /*
        private void TriggerUnclickedHandler(object sender, ClickedEventArgs clickedEventArgs)
        {
            if (_state == States.DraggingGizmo)
            {
                _currentHoverGizmo.OnDragCompleted(this);
                _state = States.Default;
            }
            else if (_state == States.PointerCapturedOnClickable)
            {
                _capturedClickTarget.PointerUntriggered(_laserPointer);
                _capturedClickTarget = null;
                _state = States.Default;
                _laserPointer.IsLockedAtTarget = false;
            }
        }

        private void ControllerGrippedHandler(object sender, ClickedEventArgs e)
        {
            if (_state == States.Default)
                AnnotationManager.Instance.GoToNextAnnotation(_teleportation);
        }
        
        
        private void MenuButtonUnclickedHandler(object sender, ClickedEventArgs e)
        {
           _laserPointer.SetLaserpointerEnabled(true);
        }
        */
        
        private void MenuButtonClickedHandler(InteractionSourceKind source, int tapCount, Ray headRay)
        {
            if(tapCount==2)
                _laserPointer.SetLaserpointerEnabled(false);
        }

        InteractiveGizmo GetGizmoUnderController()
        {
            InteractiveGizmo newHoverGizmo = null;
            var position = transform.position;
            foreach (var collider in GizmoColliders)
            {
                if (collider.bounds.Contains(position))
                {
                    newHoverGizmo = collider.gameObject.GetComponent<InteractiveGizmo>();
                    Debug.Log(">>>> collider under controller:" + collider.gameObject, this);
                    break;
                }
            }

            return newHoverGizmo;
        }

        // ToDo: will be used later
        // void SendPositionToGizmos()
        // {
        //     var position = transform.position;
        //     foreach (var collider in GizmoColliders)
        //     {
        //         if (!collider.gameObject.activeSelf)
        //             continue;

        //         var newHoverGizmo = collider.gameObject.GetComponent<InteractiveGizmo>();
        //         newHoverGizmo.OnControllerPositionUpdate(this);
        //     }
        // }

        public void TriggerHapticPulse(ushort durationMicroSec = 500)
        {
            /*var controllerInput = SteamVR_Controller.Input((int)_controller.controllerIndex);
            controllerInput.TriggerHapticPulse(durationMicroSec);*/
        }

        //private Teleportation _teleportation;
        protected LaserPointer _laserPointer;
        private GameObject _laserInstance;
        protected GestureManager gestureManager;
        //protected SteamVR_TrackedController _controller;
        private InteractiveGizmo _currentHoverGizmo;
        private IClickableLaserPointerTarget _capturedClickTarget;

        private States _state = States.Default;

        public States ActiveState
        {
            get
            {
                return _state;
            }
            private set
            {
                if (value == _state)
                {
                    Debug.LogWarning("Setting state " + value + " again?");
                    return;
                }
                _state = value;
            }
        }

        public enum States
        {
            Default = 0,
            IsCollidingWithGizmo,
            DraggingGizmo,
            PointerCapturedOnClickable,
            PointerCapturedOnTeleporter
        }
    }
}
