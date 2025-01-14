using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public class Showcase : MonoBehaviour
        {
            [SerializeField]
            private Camera[] cameras;
            [SerializeField]
            private TMP_Text[] texts;
            [SerializeField]
            private float[] durations;

            [SerializeField]
            private TMP_Text tt;

            private float currentTime = 0f;
            private float nextChangeTime;
            private int currentIndex;

            private bool processTimer = true;

            private void Awake()
            {
                nextChangeTime = durations[0];
            }

            // Update is called once per frame
            void Update()
            {
                if (processTimer)
                    currentTime += Time.deltaTime;

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    NextScene();
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    processTimer = !processTimer;
                }

                if (currentTime >= nextChangeTime)
                {
                    NextScene();
                }

                string t = $"Scene change in: {((int)(nextChangeTime - currentTime)).ToString()} s" + (processTimer ? "" : " (paused)");

                tt.text = t;
            }

            private void NextScene()
            {
                int nextIndex = currentIndex + 1;

                if (nextIndex >= cameras.Length)
                    nextIndex = 0;

                cameras[currentIndex].gameObject.SetActive(false);
                texts[currentIndex].gameObject.SetActive(false);

                currentIndex = nextIndex;

                cameras[currentIndex].gameObject.SetActive(true);
                texts[currentIndex].gameObject.SetActive(true);

                if (currentIndex == 0)
                {
                    nextChangeTime = durations[0];
                    currentTime = 0f;
                }
                else
                {
                    nextChangeTime = TotalTime(currentIndex);
                    currentTime = TotalTime(currentIndex - 1);
                }
            }

            private float TotalTime(int c)
            {
                float result = 0f;

                for (int i = 0; i <= c; i++)
                {
                    result += durations[i];
                }

                //foreach (var item in durations)
                //{
                //    result += item;
                //}

                return result;
            }
        }
    }
}