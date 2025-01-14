using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BeyondLimitsStudios.VRInteractables.DrawingBoardUtils;

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public interface DrawingBoard
        {
            /// <summary>
            /// Used to setup board.
            /// </summary>
            public void Setup();

            /// <summary>
            /// Sets if drawing on the board is possible.
            /// </summary>
            /// <param name="value">True by default.</param>
            public void EnableDrawing(bool value = true);

            /// <summary>
            /// Returns if drawing is enabled on the board.
            /// </summary>
            /// <returns>Returns true if drawing is enabled. False otherwise.</returns>
            public bool IsDrawingEnabled();

            /// <summary>
            /// Called by marker using SphereCollider when starting drawing.
            /// </summary>
            /// <param name="mk">Marker that Marker component is on.</param>
            /// <param name="pos">World position of marker.</param>
            /// <param name="size">Size of marker.</param>
            /// <param name="color">Color of marker.</param>
            public void StartDrawing(Marker mk, Vector3 pos, float size, Color color);

            /// <summary>
            /// Called by marker using MeshCollider when starting drawing.
            /// </summary>
            /// <param name="mk">Marker that Marker component is on.</param>
            /// <param name="meshVerticesInfo">Local positions of marker collider vertices.</param>
            /// <param name="color">Color of marker.</param>
            public void StartDrawing(Marker mk, MeshVertexInfo[] meshVerticesInfo, Color color);

            /// <summary>
            /// Called by marker using SphereCollider when drawing.
            /// </summary>
            /// <param name="mk">Marker that Marker component is on.</param>
            /// <param name="meshVerticesInfo">Local positions of marker MeshCollider vertices.</param>
            /// <param name="size">Size of marker.</param>
            /// <param name="value">Color of marker.</param>
            public void ReciveDrawingUpdate(Marker mk, Vector3 pos, float size, Color color);

            /// <summary>
            /// Called by marker using MeshCollider when drawing.
            /// </summary>
            /// <param name="mk">Marker that Marker component is on.</param>
            /// <param name="meshVerticesInfo">Local positions of marker collider vertices.</param>
            /// <param name="size">Size of marker.</param>
            /// <param name="value">Color of marker.</param>
            public void ReciveDrawingUpdate(Marker mk, MeshVertexInfo[] meshVerticesInfo, Color color);

            /// <summary>
            /// Called by marker when ending drawing.
            /// </summary>
            /// <param name="mk">Marker that Marker component is on.</param>
            public void StopDrawing(Marker mk);

            /// <summary>
            /// Called by marker using BoxCollider when starting erasing.
            /// </summary>
            /// <param name="er">Eraser that Eraser component is on.</param>
            /// <param name="collider">BoxCollider set on Eraser.</param>
            public void StartErasing(Eraser er, BoxCollider collider);

            /// <summary>
            /// Called by marker using MeshCollider when starting erasing.
            /// </summary>
            /// <param name="go">GameObject that Eraser component is on.</param>
            /// <param name="meshVerticesInfo">Local positions of eraser MeshCollider vertices.</param>
            public void StartErasing(Eraser er, MeshVertexInfo[] meshVerticesInfo);

            /// <summary>
            /// Called by marker using BoxCollider when erasing.
            /// </summary>
            /// <param name="er">Eraser that Eraser component is on.</param>
            /// <param name="collider">BoxCollider set on Eraser.</param>
            public void ReciveErasingUpdate(Eraser er, BoxCollider collider);

            /// <summary>
            /// Called by marker using MeshCollider when starting erasing.
            /// </summary>
            /// <param name="er">Eraser that Eraser component is on.</param>
            /// <param name="meshVerticesInfo">Local positions of eraser MeshCollider vertices.</param>
            public void ReciveErasingUpdate(Eraser er, MeshVertexInfo[] meshVerticesInfo);

            /// <summary>
            /// Called by marker using MeshCollider when ending erasing.
            /// </summary>
            /// <param name="er">Eraser that Eraser component is on.</param>
            public void StopErasing(Eraser er);

            /// <summary>
            /// Returns distance to the board.
            /// </summary>
            /// <param name="position">Position to check distance from.</param>
            /// <returns>Returns distance from position to the board.</returns>
            public float GetDistanceToBoard(Vector3 position);

            /// <summary>
            /// Returns point closest to the board.
            /// </summary>
            /// <param name="position">Position to check closest point from.</param>
            /// <returns>Returns point closest to the position on the board.</returns>
            public Vector3 GetPointOnBoard(Vector3 position);
        }
    }
}