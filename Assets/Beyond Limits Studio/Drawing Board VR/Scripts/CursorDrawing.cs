using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public class CursorDrawing : MonoBehaviour
        {
            [SerializeField]
            private Camera cam;

            [SerializeField]
            private Marker marker;

            [SerializeField]
            private Eraser eraser;

            private Vector3? markerPos;
            private Vector3? eraserPos;

            private void Awake()
            {
                if (cam == null)
                {
                    cam = Camera.main;
                }

                if (marker != null)
                    markerPos = marker.transform.position;

                if (eraser != null)
                    eraserPos = eraser.transform.position;
            }

            // Update is called once per frame
            void Update()
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (marker == null)
                        return;

                    marker.transform.position = markerPos.Value;
                }

                if (Input.GetMouseButtonUp(1))
                {
                    if (eraser == null)
                        return;

                    eraser.transform.position = eraserPos.Value;
                }

                if (Input.GetMouseButton(0))
                {
                    if (marker == null)
                    {
                        Debug.LogWarning("You have to assign marker in inspector!");
                        return;
                    }

                    if (!markerPos.HasValue)
                        markerPos = marker.transform.position;

                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, 1 << 8))
                    {
                        marker.transform.position = hit.point;
                    }
                }

                if (Input.GetMouseButton(1))
                {
                    if (eraser == null)
                    {
                        Debug.LogWarning("You have to assign eraser in inspector!");
                        return;
                    }

                    if (!eraserPos.HasValue)
                        eraserPos = eraser.transform.position;

                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, 1 << 8))
                    {
                        eraser.transform.position = hit.point;
                    }
                }
            }
        }
    }
}