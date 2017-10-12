using System.Collections;
using System.Collections.Generic;
using ff.vr.interaction;
using UnityEngine;

namespace ff.vr.annotate.viz
{
    public class AnnotationTourNeighbourButton : MonoBehaviour, IClickableLaserPointerTarget/*, ITeleportationTrigger*/
    {
        [SerializeField]
        private OnTeleportationArrivalOrientation _onTeleportationArrivalOrientationPrefab;
        [SerializeField]
        private AnnotationGizmo _attachedGizmo;

        public bool InverseDirection;

        void ILaserPointerTarget.PointerEnter(LaserPointer pointer)
        {
            SelectionManager.Instance.SetOnAnnotationGizmoHover(GetTargetGizmo());
        }

        void ILaserPointerTarget.PointerExit(LaserPointer pointer)
        {
            SelectionManager.Instance.SetOnAnnotationGizmoUnhover(GetTargetGizmo());
        }

        void IClickableLaserPointerTarget.PointerTriggered(LaserPointer pointer)
        {
            SelectionManager.Instance.SetSelectedItem(GetTargetGizmo());
        }

        void IClickableLaserPointerTarget.PointerUntriggered(LaserPointer pointer)
        {
        }

        void ILaserPointerTarget.PointerUpdate(LaserPointer pointer)
        {
        }

        public AnnotationGizmo GetTargetGizmo()
        {
            return
            InverseDirection ?
            AnnotationManager.Instance.GetPreviousAnnotationGizmoOnNode(_attachedGizmo) :
            AnnotationManager.Instance.GetNextAnnotationGizmoOnNode(_attachedGizmo);
        }

        public Vector3 GetTeleportationTarget(AnnotationGizmo gizmo)
        {
            var teleportationTarget = gizmo.Annotation.ViewPointPosition.position;
            return new Vector3(teleportationTarget.x, 0, teleportationTarget.z);
        }

    }

}