using BeyondLimitsStudios.VRInteractables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public class ImageSaveVR : MonoBehaviour
        {
            [SerializeField]
            private DrawingBoardTexture board;
            [SerializeField]
            private string fileName;
            [SerializeField]
            private Color color;
            [SerializeField]
            private Texture2D tex;

            public void SaveImage()
            {
                board.SaveImage(Application.persistentDataPath, fileName);
            }

            public void SetBackground()
            {
                board.SetBackgroundColor(color);
                board.ClearBoard();
            }

            public void ClearBoard()
            {
                board.ClearBoard();
            }

            public void SetTexture()
            {
                board.SetImage(tex);
            }
        }
    }
}