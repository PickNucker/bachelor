using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public class MovingCloud : MonoBehaviour
        {
            private static Vector3 windDirection = new Vector3(-1f, 0f, -0.8f);
            private static float globalWindSpeed = 1f;
            private float windSpeed = 1f;
            private static float distance = 50f;

            private void Awake()
            {
                windSpeed = Random.Range(0.5f, 0.8f);
            }

            private void Update()
            {
                this.transform.position += windDirection.normalized * globalWindSpeed * windSpeed * Time.deltaTime;
            }

            private void OnTriggerEnter(Collider other)
            {
                this.transform.position -= windDirection.normalized * distance;
                windSpeed = Random.Range(0.5f, 0.8f);
            }
        }
    }
}