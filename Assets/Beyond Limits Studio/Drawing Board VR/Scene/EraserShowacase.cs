using BeyondLimitsStudios.VRInteractables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public class EraserShowacase : MonoBehaviour
        {
            [SerializeField]
            private float sinSpeed = 2f;
            [SerializeField]
            private float ySpeed = 0.1f;
            [SerializeField]
            private float yLimit = 1f;

            [SerializeField]
            private DrawingBoardTexture drawingBoard;

            private Texture2D image;

            private void Awake()
            {

            }

            private void Update()
            {
                if (image == null)
                    image = drawingBoard.GetSourceTexture();

                Vector3 localPos = this.transform.GetChild(0).localPosition;
                Vector3 localRot = this.transform.localRotation.eulerAngles;

                float sin = Mathf.Sin(Time.realtimeSinceStartup * sinSpeed);
                sin += 1f;
                sin /= 2;

                localRot.z = sin * 90;

                localPos.y += Time.deltaTime * ySpeed * sin;

                if (this.transform.GetChild(0).localPosition.y > yLimit)
                {
                    localPos.y = 0f;
                    drawingBoard.SetRenderTexture(image);
                }

                this.transform.localRotation = Quaternion.Euler(localRot);
                this.transform.GetChild(0).localPosition = localPos;
            }
        }
    }
}