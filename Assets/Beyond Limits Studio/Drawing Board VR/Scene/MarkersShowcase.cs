using BeyondLimitsStudios.VRInteractables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public class MarkersShowcase : MonoBehaviour
        {
            [SerializeField]
            private float sinSpeed = 10f;
            [SerializeField]
            private float xSpeed = 0.5f;
            [SerializeField]
            private float xLimit = 1f;
            [SerializeField]
            private float yAmplitude = 0.1f;
            [SerializeField]
            private float zAmplitude = 0.01f;

            Marker[] markers;

            private void Awake()
            {
                markers = this.transform.GetComponentsInChildren<Marker>();
            }

            private void Update()
            {
                Vector3 localPos = this.transform.localPosition;

                float sin = Mathf.Sin(Time.realtimeSinceStartup * sinSpeed);

                localPos.y = sin * yAmplitude;
                localPos.z = sin * zAmplitude;
                localPos.x += Time.deltaTime * xSpeed;

                if (this.transform.localPosition.x > xLimit)
                {
                    localPos.x = 0f;

                    foreach (var item in markers)
                    {
                        item.SetColor(Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f));
                    }
                }

                this.transform.localPosition = localPos;
            }
        }
    }
}