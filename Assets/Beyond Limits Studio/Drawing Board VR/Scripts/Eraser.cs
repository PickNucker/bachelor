using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BeyondLimitsStudios.VRInteractables.DrawingBoardUtils;

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public class Eraser : MonoBehaviour
        {
            [SerializeField]
            [Tooltip("Does the eraser use BoxCollider or MeshCollider.")]
            private bool isBox = true;

            [SerializeField]
            [Tooltip("BoxCollider used to detect collision with board (used only if isBox is enabled).")]
            private BoxCollider boxCollider;
            [SerializeField]
            [Tooltip("MeshCollider used to detect collision with board (used only if isBox is disabled).")]
            private MeshCollider meshCollider;

            private MeshVertexInfo[] verticesInfo;

            private Dictionary<GameObject, DrawingBoard> boards = new Dictionary<GameObject, DrawingBoard>();

            private void Awake()
            {
                if (isBox)
                {
                    if (boxCollider == null)
                    {
                        boxCollider = this.GetComponentInChildren<BoxCollider>();

                        if (boxCollider == null)
                        {
                            Debug.LogError("Couldn't find Box Collider component on Eraser GameObject!");
                            this.enabled = false;
                            return;
                        }
                    }
                }
                else
                {
                    if (meshCollider == null)
                    {
                        meshCollider = this.GetComponentInChildren<MeshCollider>();

                        if (meshCollider == null)
                        {
                            Debug.LogError("Couldn't find Mesh Collider component on Eraser GameObject!");
                            this.enabled = false;
                            return;
                        }
                    }

                    // verticesInfo = GetMeshVertexInfo(meshCollider);
                    if (DrawingBoardSettings.computedMeshes.ContainsKey(meshCollider.sharedMesh))
                    {
                        verticesInfo = DrawingBoardSettings.computedMeshes[meshCollider.sharedMesh];
                        Debug.Log($"Mesh {meshCollider.sharedMesh} already present in dictionary.");
                    }
                    else
                    {
                        verticesInfo = GetMeshVertexInfo(meshCollider);
                        verticesInfo = SimplifyMeshVertexInfo(verticesInfo);

                        DrawingBoardSettings.computedMeshes.Add(meshCollider.sharedMesh, verticesInfo);

                        Debug.Log($"Added new mesh {meshCollider.sharedMesh} to dictionary.");
                    }

                    if (verticesInfo.Length > DrawingBoardSettings.MaxMeshColliderVertices)
                    {
                        Debug.LogWarning($"MeshCollider has {verticesInfo.Length} vertices. Limit is {DrawingBoardSettings.MaxMeshColliderVertices}.");
                        meshCollider.enabled = false;
                    }
                }
            }

            private void OnTriggerEnter(Collider other)
            {
                if (other.gameObject.layer == DrawingBoardSettings.DrawingBoardLayer)
                {
                    boards.Add(other.gameObject, other.gameObject.GetComponent<DrawingBoard>());
                    if (isBox)
                        boards[other.gameObject].StartErasing(this, boxCollider);
                    else
                        boards[other.gameObject].StartErasing(this, verticesInfo);
                }
            }

            private void OnTriggerStay(Collider other)
            {
                if (other.gameObject.layer == DrawingBoardSettings.DrawingBoardLayer)
                {
                    if (isBox)
                        boards[other.gameObject].ReciveErasingUpdate(this, boxCollider);
                    else
                        boards[other.gameObject].ReciveErasingUpdate(this, verticesInfo);
                }
            }

            private void OnTriggerExit(Collider other)
            {
                if (other.gameObject.layer == DrawingBoardSettings.DrawingBoardLayer)
                {
                    boards[other.gameObject].StopErasing(this);
                    boards.Remove(other.gameObject);
                }
            }

            public void DeleteBoard(GameObject go)
            {
                boards.Remove(go);
            }
        }
    }
}
