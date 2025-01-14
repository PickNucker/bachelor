using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BeyondLimitsStudios.VRInteractables.DrawingBoardUtils;

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public class DrawingBoardSettings
        {
            [Tooltip("Static string. Used to get correct texture on DrawingBoard material.")]
            // For URP
            public static string DefaultTextureName = "_BaseMap";
            // For HDRP
            // public static string DefaultTextureName = "_BaseColorMap";
            // For Built-in
            // public static string DefaultTextureName = "_MainTex";

            [Tooltip("Static int. Layer of the drawing baord.")]
            public static int DrawingBoardLayer = 8;

            [Tooltip("Static int. Max number of vertices when using MeshCollider.")]
            public static int MaxMeshColliderVertices = 512;
            [Tooltip("Static int. Max number of contact points when using MeshCollider.")]
            public static int MaxContactPoints = 128;

            [Tooltip("Static Dictionary<Mesh, MeshVertexInfo[]> to keep track of already calculated meshes for markers and erasers.")]
            public static Dictionary<Mesh, MeshVertexInfo[]> computedMeshes = new Dictionary<Mesh, MeshVertexInfo[]>();
        }
    }
}