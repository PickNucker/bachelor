using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static BeyondLimitsStudios.VRInteractables.DrawingBoardUtils;

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public class Marker : NetworkBehaviour
        {
            [SerializeField]
            [Tooltip("Does the marker use SphereCollider or MeshCollider.")]
            private bool isSphere = true;
            [SerializeField]
            [Tooltip("Should size of the marker always be constant or it should vary depending on SphereCollider center distance to board (used only for SphereCollider).")]
            private bool useConstantSize = true;

            [SerializeField]
            [Tooltip("Size of marker if useConstantSize is enabled.")]
            private float size = 0.01f;
            [SerializeField]
            [Tooltip("Color of marker.")]
            private Color color = Color.white;

            [SerializeField]
            [Tooltip("SphereCollider used to detect collision with board (used only if isSphere is enabled).")]
            private SphereCollider sphereCollider;
            [SerializeField]
            [Tooltip("MeshCollider used to detect collision with board (used only if isSphere is disabled).")]
            private MeshCollider meshCollider;

            private MeshVertexInfo[] verticesInfo;

            private Dictionary<GameObject, DrawingBoard> boards = new Dictionary<GameObject, DrawingBoard>();

            private void Awake()
            {
                if(IsServer) GetComponent<NetworkObject>().Spawn();

                if (isSphere)
                {
                    if (sphereCollider == null)
                    {
                        sphereCollider = this.GetComponentInChildren<SphereCollider>();

                        if (sphereCollider == null)
                        {
                            Debug.LogError("Couldn't find Sphere Collider component on Marker GameObject!");
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
                            Debug.LogError("Couldn't find Mesh Collider component on Marker GameObject!");
                            this.enabled = false;
                            return;
                        }
                    }                    

                    if (DrawingBoardSettings.computedMeshes.ContainsKey(meshCollider.sharedMesh))
                    {
                        verticesInfo = DrawingBoardSettings.computedMeshes[meshCollider.sharedMesh];
                        // Debug.Log($"Mesh {meshCollider.sharedMesh} already present in dictionary.");
                    }
                    else
                    {
                        verticesInfo = GetMeshVertexInfo(meshCollider);
                        verticesInfo = SimplifyMeshVertexInfo(verticesInfo);

                        DrawingBoardSettings.computedMeshes.Add(meshCollider.sharedMesh, verticesInfo);
                        // Debug.Log($"Added new mesh {meshCollider.sharedMesh} to dictionary.");
                    }

                    if (verticesInfo.Length > DrawingBoardSettings.MaxMeshColliderVertices)
                    {
                        Debug.LogWarning($"MeshCollider has {verticesInfo.Length} vertices. Limit is {DrawingBoardSettings.MaxMeshColliderVertices}.");
                        meshCollider.enabled = false;
                    }
                }
            }

            public void SetColor(Color col)
            {
                color = col;
            }

            private void OnTriggerEnter(Collider other)
            {
                if (other.gameObject.layer == DrawingBoardSettings.DrawingBoardLayer)
                {
                    DrawingBoard board;

                    if (!other.gameObject.TryGetComponent<DrawingBoard>(out board))
                    {
                        // Debug.LogError($"Marker collided with a GameObject on the DrawingBoard layer (Layer {DrawingBoardSettings.DrawingBoardLayer}) that doesn't have a DrawingBoard component. This error can occur if the layer setup is incorrect, such as having other objects on the DrawingBoard layer that are not actual drawing boards, or if you forgot to add the DrawingBoard component to the intended drawing board GameObject. If you want to change the DrawingBoard layer, you can modify it in the \"DrawingBoardSettings\" script. Please ensure that the layer assignments are set up correctly and that all drawing board GameObjects have the DrawingBoard component attached.");
                        Debug.LogError($"Marker collided with a GameObject on the DrawingBoard layer (Layer {DrawingBoardSettings.DrawingBoardLayer}) that lacks a DrawingBoard component. Check the layer setup and ensure the intended drawing board GameObject has the \"DrawingBoard\" component attached. To change the DrawingBoard layer, modify the \"DrawingBoardSettings\" script.");
                    }

                    boards.Add(other.gameObject, board);

                    if (isSphere)
                    {
                        float s;

                        if (useConstantSize)
                        {
                            s = size;
                        }
                        else
                        {
                            float dtb = boards[other.gameObject].GetDistanceToBoard(this.transform.position);
                            float r = sphereCollider.radius * this.transform.localScale.x;
                            s = Mathf.Sqrt(r * r - dtb * dtb);
                        }
                        // s = Mathf.Sqrt(Mathf.Max(0f, Mathf.Pow(2 * sphereCollider.radius * this.transform.localScale.x, 2) - Mathf.Pow(boards[other.gameObject].GetDistanceToBoard(this.transform.position), 2)));

                        boards[other.gameObject].StartDrawing(this, this.transform.position, useConstantSize ? size : s, color);
                    }
                    else
                    {
                        boards[other.gameObject].StartDrawing(this, verticesInfo, color);
                    }

                    //if (isSphere)
                    //    boards[other.gameObject].StartDrawing(this.gameObject, this.transform.position, useConstantSize ? size : 0.25f * Mathf.Sqrt(Mathf.Max(0f, Mathf.Pow(2 * sphereCollider.radius * this.transform.localScale.x, 2) - Mathf.Pow(boards[other.gameObject].GetDistanceToBoard(this.transform.position), 2))), color);
                    //else
                    //    boards[other.gameObject].StartDrawing(this.gameObject, verticesInfo, color);
                }
            }

            private void OnTriggerStay(Collider other)
            {
                if (other.gameObject.layer == DrawingBoardSettings.DrawingBoardLayer)
                {
                    if (isSphere)
                    {
                        float s;

                        if (useConstantSize)
                        {
                            s = size;
                        }
                        else
                        {
                            float dtb = boards[other.gameObject].GetDistanceToBoard(this.transform.position);
                            float r = sphereCollider.radius * this.transform.localScale.x;

                            s = Mathf.Sqrt(r * r - dtb * dtb);
                        }
                        // s = Mathf.Sqrt(Mathf.Max(0f, Mathf.Pow(2 * sphereCollider.radius * this.transform.localScale.x, 2) - Mathf.Pow(boards[other.gameObject].GetDistanceToBoard(this.transform.position), 2)));

                        boards[other.gameObject].ReciveDrawingUpdate(this, this.transform.position, useConstantSize ? size : s, color);
                    }
                    else
                    {
                        boards[other.gameObject].ReciveDrawingUpdate(this, verticesInfo, color);
                    }
                }
            }

            private void OnTriggerExit(Collider other)
            {
                if (other.gameObject.layer == DrawingBoardSettings.DrawingBoardLayer)
                {
                    boards[other.gameObject].StopDrawing(this);
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