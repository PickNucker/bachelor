using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public class TextAnimation : MonoBehaviour
        {
            private Material material;
            [SerializeField]
            private float speed = 1f;
            float h;
            float s;
            float v;

            private void Awake()
            {
                material = this.GetComponentInChildren<TMP_Text>().fontSharedMaterial;
                // Color.RGBToHSV(material.GetColor("_OutlineColor"), out h, out s, out v);
                Color.RGBToHSV(Color.red, out h, out s, out v);
            }

            private void Update()
            {
                h += Time.deltaTime * speed;
                if (h > 1) h -= 1;
                material.SetColor("_OutlineColor", Color.HSVToRGB(h, s, v, true));
            }
        }
    }
}