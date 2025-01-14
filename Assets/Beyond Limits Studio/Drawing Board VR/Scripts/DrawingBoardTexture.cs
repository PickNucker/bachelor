using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static BeyondLimitsStudios.VRInteractables.DrawingBoardUtils;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public class DrawingBoardTexture : NetworkBehaviour, DrawingBoard
        {
            [SerializeField]
            [Tooltip("The value determines if you can draw on the board. DO NO CHANGE THAT VALUE AT RUNTIME. Use EnableDrawing(bool value) function instead.")]
            private bool drawingEnabled = true;

            // [SerializeField]
            [Tooltip("Object on which you want to render image.")]
            private Transform drawObject;

            [SerializeField]
            [Tooltip("Determines where the image png file will be save when you call SaveTextureToFile() function with no parameters.")]
            private string filePath = "";
            [SerializeField]
            [Tooltip("Determines how the image png file will be called when you call SaveTextureToFile() function with no parameters (.png extension is added automatically).")]
            private string fileName = "";

            [SerializeField]
            [Tooltip("If you want to use different shader than default HDRP, URP or Built-in render pipeline material you need to enter color texture property name here.")]
            private string textureName = "";

            [SerializeField]
            [Tooltip("Sets color of board background. When erasing the color will be applied to texture.")]
            private Color backgroundColor = new Color(0f, 0f, 0f, 0f);

            [SerializeField]
            [Tooltip("Determines if sourceTexture will be applied to the board on setup.")]
            private bool useSourceTexture = false;

            [SerializeField]
            [Tooltip("Image that will be applied to board on setup if useSourceTexture is enabled.")]
            private Texture2D sourceTexture;

            // [SerializeField]
            [Tooltip("If you don't want to create new RenderTexture on setup you can use one already created.")]
            private RenderTexture renderTexture;

            private int cornersBufferSize = 64;
            private ComputeBuffer cornersBuffer;

            // [SerializeField]
            [Tooltip("If enabled OnDrawGizmos will draw wireless spheres in corners of board.")]
            private bool debug = false;

            [SerializeField]
            [Tooltip("Determines minimal distance that marker has to move to update board if using SphereCollider on marker.")]
            private float drawUpdateThreshold = 0.001f;

            [SerializeField]
            [Tooltip("Determines width (x value) of render texture created on setup. ResolutionY (height) is calculated automatically so pixels are even size.")]
            private int resolutionX = 256;

            private int resolutionY;

            [SerializeField]
            private Dictionary<Marker, Vector3> drawInfo = new Dictionary<Marker, Vector3>();

            [SerializeField]
            private Dictionary<Marker, Vector2Int[]> meshDrawInfo = new Dictionary<Marker, Vector2Int[]>();

            [SerializeField]
            private Dictionary<Eraser, Vector2Int[]> eraseInfo = new Dictionary<Eraser, Vector2Int[]>();

            //[SerializeField]
            //private Dictionary<GameObject, Vector3> drawInfo = new Dictionary<GameObject, Vector3>();

            //[SerializeField]
            //private Dictionary<GameObject, Vector2Int[]> meshDrawInfo = new Dictionary<GameObject, Vector2Int[]>();

            //[SerializeField]
            //private Dictionary<GameObject, Vector2Int[]> eraseInfo = new Dictionary<GameObject, Vector2Int[]>();

            private Vector3[] corners = new Vector3[4];
            private Plane plane;

            List<Vector2Int> verticesOnTexture = new List<Vector2Int>();
            Vector2Int[] currentVertices = new Vector2Int[128];
            List<Vector2Int> currentVerticesList = new List<Vector2Int>();

            [SerializeField]
            [Tooltip("Reference to compute shader used for calculations. Don't change it. It should always be DrawingBoardComputeShader.")]
            private ComputeShader computeShader;

            private void Awake()
            {
                if (IsServer) GetComponent<NetworkObject>().Spawn();

                Setup();
            }

            public void Setup()
            {
                this.gameObject.layer = DrawingBoardSettings.DrawingBoardLayer;

                if (drawObject == null)
                {
                    drawObject = this.transform;
                }

                int i = 0;
                foreach (var item in this.GetComponent<MeshCollider>().sharedMesh.vertices)
                {
                    corners[i] = item;
                    i++;
                }

                if (useSourceTexture)
                {
                    if (sourceTexture == null)
                    {
                        Debug.LogWarning("SourceTexture is not assigned!");
                        CreateEmptyBoard();
                    }
                    else
                    {
                        SetRenderTexture(sourceTexture);
                    }
                }
                else
                {
                    CreateEmptyBoard();
                }

                // drawObject.GetComponent<Renderer>().material.SetTexture("_BaseMap", renderTexture);
                if (string.IsNullOrEmpty(textureName))
                {
                    drawObject.GetComponent<Renderer>().material.SetTexture(DrawingBoardSettings.DefaultTextureName, renderTexture);
                }
                else
                {
                    drawObject.GetComponent<Renderer>().material.SetTexture(textureName, renderTexture);
                }
                    

                cornersBuffer = new ComputeBuffer(cornersBufferSize, sizeof(int) * 2);
            }

            public void EnableDrawing(bool value = true)
            {
                drawingEnabled = value;

                if (value == false)
                {
                    List<Marker> list = new List<Marker>();

                    foreach (var marker in drawInfo)
                    {
                        list.Add(marker.Key);
                    }

                    foreach (var marker in meshDrawInfo)
                    {
                        list.Add(marker.Key);
                    }

                    foreach (var marker in list)
                    {
                        marker.DeleteBoard(this.gameObject);
                        StopDrawing(marker);
                    }

                    List<Eraser> list2 = new List<Eraser>();

                    foreach (var eraser in eraseInfo)
                    {
                        list2.Add(eraser.Key);
                    }

                    foreach (var eraser in list2)
                    {
                        eraser.DeleteBoard(this.gameObject);
                        StopErasing(eraser);
                    }
                }
            }

            public bool IsDrawingEnabled()
            {
                return drawingEnabled;
            }

            /// <summary>
            /// Creates empty board if useSourceTexture is set to false.
            /// </summary>
            public void CreateEmptyBoard()
            {
                if (renderTexture == null)
                {
                    float proportion = Vector3.Distance(this.transform.TransformPoint(corners[2]), this.transform.TransformPoint(corners[0])) / Vector3.Distance(this.transform.TransformPoint(corners[1]), this.transform.TransformPoint(corners[0]));
                    resolutionY = (int)(resolutionX * proportion);
                    renderTexture = new RenderTexture(resolutionX, resolutionY, 32);
                    renderTexture.enableRandomWrite = true;
                    renderTexture.Create();
                    // drawObject.GetComponent<Renderer>().material.SetTexture("_BaseMap", renderTexture);
                }
                else
                {
                    resolutionX = renderTexture.width;
                    resolutionY = renderTexture.height;
                    renderTexture.enableRandomWrite = true;
                }


                ClearBoard();
            }

            public void SetRenderTexture(Texture2D tex, bool resize = false)
            {
                SetImage(tex, resize);
            }

            /// <summary>
            /// Sets image on the board.
            /// </summary>
            /// <param name="tex">Texture that will be applied to the board.</param>
            /// <param name="resize">If set to true imahe resolution will be set to tex resolution.</param>
            public void SetImage(Texture2D tex, bool resize = false)
            {
                if (tex == null)
                {
                    Debug.LogWarning("Tex is not assigned!");
                    return;
                }

                if (renderTexture == null || (resize && (renderTexture.width != tex.width || renderTexture.height != tex.height)))
                {
                    if (resize)
                    {
                        resolutionX = tex.width;
                        resolutionY = tex.height;
                    }
                    else
                    {
                        float proportion = Vector3.Distance(this.transform.TransformPoint(corners[2]), this.transform.TransformPoint(corners[0])) / Vector3.Distance(this.transform.TransformPoint(corners[1]), this.transform.TransformPoint(corners[0]));
                        resolutionY = (int)(resolutionX * proportion);
                    }
                    
                    if (renderTexture != null)
                    {
                        renderTexture.Release();
                    }

                    renderTexture = new RenderTexture(resolutionX, resolutionY, 32);
                    renderTexture.enableRandomWrite = true;
                    renderTexture.Create();
                    // drawObject.GetComponent<Renderer>().material.SetTexture("_BaseMap", renderTexture);
                    if (string.IsNullOrEmpty(textureName))
                    {
                        drawObject.GetComponent<Renderer>().material.SetTexture(DrawingBoardSettings.DefaultTextureName, renderTexture);
                    }
                    else
                    {
                        drawObject.GetComponent<Renderer>().material.SetTexture(textureName, renderTexture);
                    }
                }
                
                Graphics.Blit(tex, renderTexture);
            }

            public Texture2D GetSourceTexture()
            {
                return sourceTexture;
            }

            /// <summary>
            /// Returns board background color on the board.
            /// </summary>
            public Color GetBackgroundColor()
            {
                return backgroundColor;
            }

            /// <summary>
            /// Sets background color on the board.
            /// </summary>
            public void SetBackgroundColor(Color col)
            {
                backgroundColor = col;
            }

            /// <summary>
            /// Clears board. Sets backgroundColor on entire texture.
            /// </summary>
            public void ClearBoard()
            {
                computeShader.SetVector("Color", backgroundColor);

                computeShader.SetTexture(4, "tex", renderTexture);

                computeShader.Dispatch(4, renderTexture.width / 8 + 1, renderTexture.height / 8 + 1, 1);              
            }
                        
            public void StartDrawing(Marker mk, Vector3 pos, float size, Color color)
            {
                if (!drawingEnabled)
                    return;

                pos = ProjectPointOnPlane(this.transform.TransformPoint(corners[0]), this.transform.TransformPoint(corners[1]), this.transform.TransformPoint(corners[2]), pos);
                Vector3 position = this.transform.InverseTransformPoint(pos);

                Draw(position, size, color);

                drawInfo.Add(mk, position);
            }
                        
            public void StartDrawing(Marker mk, MeshVertexInfo[] meshVerticesInfo, Color color)
            {
                //if (!drawingEnabled)
                //    return;

                //Vector2Int[] currentVertices = GetVerticesOnTexture(meshVerticesInfo, go.transform);

                //Draw(currentVertices, color);

                //meshDrawInfo.Add(go, currentVertices);

                if (!drawingEnabled)
                    return;

                int c = GetVerticesOnTexture(meshVerticesInfo, mk.transform, ref currentVertices);

                Draw(currentVertices, c, color);

                meshDrawInfo.Add(mk, new Vector2Int[c]);

                for (int i = 0; i < c; i++)
                {
                    meshDrawInfo[mk][i] = currentVertices[i];
                }
            }
                        
            public void ReciveDrawingUpdate(Marker mk, Vector3 pos, float size, Color color)
            {
                if (!drawingEnabled)
                    return;

                pos = ProjectPointOnPlane(this.transform.TransformPoint(corners[0]), this.transform.TransformPoint(corners[1]), this.transform.TransformPoint(corners[2]), pos);
                Vector3 position = this.transform.InverseTransformPoint(pos);

                if (drawInfo[mk] == position)
                {
                    return;
                }

                if (Vector3.SqrMagnitude(drawInfo[mk] - position) < drawUpdateThreshold * drawUpdateThreshold)
                {
                    return;
                }

                Draw(position, size, color, drawInfo[mk]);

                drawInfo[mk] = position;
            }
                        
            public void ReciveDrawingUpdate(Marker mk, MeshVertexInfo[] meshVerticesInfo, Color color)
            {
                if (!drawingEnabled)
                    return;

                int c = GetVerticesOnTexture(meshVerticesInfo, mk.transform, ref currentVertices);

                if (c == meshDrawInfo[mk].Length)
                {
                    bool isDifferent = false;

                    for (int i = 0; i < meshDrawInfo[mk].Length; i++)
                    {
                        if (currentVertices[i] != meshDrawInfo[mk][i])
                        {
                            isDifferent = true;
                            break;
                        }
                    }

                    if (!isDifferent)
                    {
                        return;
                    }
                }

                Draw(currentVertices, c, color, meshDrawInfo[mk]);

                if (meshDrawInfo[mk].Length != c)
                {
                    meshDrawInfo[mk] = new Vector2Int[c];
                }

                for (int i = 0; i < c; i++)
                {
                    meshDrawInfo[mk][i] = currentVertices[i];
                }
            }
                        
            public void StopDrawing(Marker mk)
            {
                if (!drawingEnabled)
                    return;

                if (drawInfo.ContainsKey(mk))
                    drawInfo.Remove(mk);

                if (meshDrawInfo.ContainsKey(mk))
                    meshDrawInfo.Remove(mk);
            }
                        
            public void StartErasing(Eraser er, BoxCollider collider)
            {
                if (!drawingEnabled)
                    return;

                int c = GetVerticesOnTexture(collider, ref currentVertices);

                Erase(currentVertices, c);

                eraseInfo.Add(er, new Vector2Int[c]);

                for (int i = 0; i < c; i++)
                {
                    eraseInfo[er][i] = currentVertices[i];
                }

                // eraseInfo.Add(go, currentVertices);
            }
                        
            public void StartErasing(Eraser er, MeshVertexInfo[] meshVerticesInfo)
            {
                if (!drawingEnabled)
                    return;

                // Vector2Int[] currentVertices = GetVerticesOnTexture(meshVerticesInfo, go.transform);
                int c = GetVerticesOnTexture(meshVerticesInfo, er.transform, ref currentVertices);

                Erase(currentVertices, c);

                eraseInfo.Add(er, new Vector2Int[c]);

                for (int i = 0; i < c; i++)
                {
                    eraseInfo[er][i] = currentVertices[i];
                }
            }
                        
            public void ReciveErasingUpdate(Eraser er, BoxCollider collider)
            {
                if (!drawingEnabled)
                    return;

                // Vector2Int[] currentVertices = GetVerticesOnTexture(collider);
                int c = GetVerticesOnTexture(collider, ref currentVertices);

                if (c == eraseInfo[er].Length)
                {
                    bool isDifferent = false;

                    for (int i = 0; i < eraseInfo[er].Length; i++)
                    {
                        if (currentVertices[i] != eraseInfo[er][i])
                        {
                            isDifferent = true;
                            break;
                        }
                    }

                    if (!isDifferent)
                    {
                        return;
                    }
                }

                Erase(currentVertices, c, eraseInfo[er]);

                if (eraseInfo[er].Length != c)
                {
                    eraseInfo[er] = new Vector2Int[c];
                }

                for (int i = 0; i < c; i++)
                {
                    eraseInfo[er][i] = currentVertices[i];
                }

                // eraseInfo[go] = currentVertices;
            }
                        
            public void ReciveErasingUpdate(Eraser er, MeshVertexInfo[] meshVerticesInfo)
            {
                if (!drawingEnabled)
                    return;

                int c = GetVerticesOnTexture(meshVerticesInfo, er.transform, ref currentVertices);

                if (c == eraseInfo[er].Length)
                {
                    bool isDifferent = false;

                    for (int i = 0; i < eraseInfo[er].Length; i++)
                    {
                        if (currentVertices[i] != eraseInfo[er][i])
                        {
                            isDifferent = true;
                            break;
                        }
                    }

                    if (!isDifferent)
                    {
                        return;
                    }
                }

                Erase(currentVertices, c, eraseInfo[er]);

                if (eraseInfo[er].Length != c)
                {
                    eraseInfo[er] = new Vector2Int[c];
                }

                for (int i = 0; i < c; i++)
                {
                    eraseInfo[er][i] = currentVertices[i];
                }
            }
                        
            public void StopErasing(Eraser er)
            {
                if (!drawingEnabled)
                    return;

                if (cornersBuffer.count > cornersBufferSize)
                {
                    Debug.Log("Creating new corners buffer");
                    cornersBuffer.Release();
                    cornersBuffer = new ComputeBuffer(cornersBufferSize, sizeof(int) * 2);
                }

                eraseInfo.Remove(er);
            }
                        
            public float GetDistanceToBoard(Vector3 position)
            {
                return Vector3.Distance(position, GetPointOnBoard(position));
            }
                        
            public Vector3 GetPointOnBoard(Vector3 position)
            {
                return ProjectPointOnPlane(this.transform.TransformPoint(corners[0]), this.transform.TransformPoint(corners[1]), this.transform.TransformPoint(corners[2]), position);
            }

            /// <summary>
            /// Saves current texture on board to png file using filePath save path and fileName name.
            /// </summary>
            public bool SaveTextureToFile()
            {
                return SaveImage();
            }

            /// <summary>
            /// Saves current texture on board to png file using given save path and name.
            /// </summary>
            /// <param name="path">Path where the file will be saved.</param>
            /// <param name="name">Name of the file (.png extension is added automatically).</param>
            public bool SaveTextureToFile(string path, string name)
            {
                return SaveImage(path, name);
            }

            /// <summary>
            /// Saves current texture on board to png file using filePath save path and fileName name.
            /// </summary>
            public bool SaveImage()
            {
                if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileName))
                {
                    Debug.LogWarning("You need to set filePath and fileName in the inspector!");
                    return false;
                }

                return SaveImage(filePath, fileName);
            }

            /// <summary>
            /// Saves current texture on board to png file using given save path and name.
            /// </summary>
            /// <param name="path">Path where the file will be saved.</param>
            /// <param name="name">Name of the file (.png extension is added automatically).</param>
            public bool SaveImage(string path, string name)
            {
                if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(name))
                {
                    Debug.LogWarning("FilePath and fileName cannot be empty!");
                    return false;
                }

                if (!Directory.Exists(path))
                {
                    Debug.LogWarning($"Directory {path} does not exist!");
                    return false;
                }

                if (renderTexture == null)
                {
                    Debug.LogWarning($"Render texture does not exist!");
                    return false;
                }

                // Convert the texture to PNG
                byte[] bytes = GetTexture().EncodeToPNG();

                // Save the bytes to file

            #if UNITY_ANDROID
                //NativeGallery.SaveImageToGallery(bytes, Path.GetDirectoryName(path), Path.GetFileName(path));
            #else
                File.WriteAllBytes(Path.Combine(path, (name + ".png")), bytes);
                Debug.Log("Texture saved to " + path);
            #endif

                return true;
            }

            /// <summary>
            /// Returns image from the board.
            /// </summary>
            /// <returns>Returns new Texture2D with pixels copied from the board.</returns>
            public Texture2D GetTexture()
            {
                if (renderTexture == null)
                    return null;

                Texture2D result = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

                RenderTexture.active = renderTexture;

                result.ReadPixels(new Rect(0, 0, result.width, result.height), 0, 0);

                RenderTexture.active = null;
                return result;
            }

            /// <summary>
            /// Erases the board in convex polygon made of currentVertices and lastVertices.
            /// </summary>
            /// <param name="currentVertices">Vertices positions when executing the function.</param>
            /// <param name="lastVertices">Vertices positions in last frame. Null by default.</param>
            private void Erase(Vector2Int[] currentVertices, Vector2Int[] lastVertices = null)
            {
                verticesOnTexture.Clear();

                foreach (var item in currentVertices)
                {
                    verticesOnTexture.Add(item);
                }

                if (lastVertices == null)
                {
                    // verticesOnTexture = ConvexHull(verticesOnTexture);
                    ConvexHull(ref verticesOnTexture);

                    SetPixelsInPolygonCS(renderTexture, verticesOnTexture, backgroundColor, computeShader, ref cornersBuffer);
                }
                else
                {
                    foreach (var item in lastVertices)
                    {
                        verticesOnTexture.Add(item);
                    }

                    // verticesOnTexture = ConvexHull(verticesOnTexture);
                    ConvexHull(ref verticesOnTexture);

                    SetPixelsInPolygonCS(renderTexture, verticesOnTexture, backgroundColor, computeShader, ref cornersBuffer);
                }
            }

            /// <summary>
            /// Erases the board in convex polygon made of currentVertices and lastVertices.
            /// </summary>
            /// <param name="currentVertices">Vertices positions when executing the function.</param>
            /// <param name="count">Nuber of vertices to use.</param>
            /// <param name="lastVertices">Vertices positions in last frame. Null by default.</param>
            private void Erase(Vector2Int[] currentVertices, int count, Vector2Int[] lastVertices = null)
            {
                // List<Vector2Int> verticesOnTexture = new List<Vector2Int>();
                verticesOnTexture.Clear();

                for (int i = 0; i < count; i++)
                {
                    verticesOnTexture.Add(currentVertices[i]);
                }

                //foreach (var item in currentVertices)
                //{
                //    verticesOnTexture.Add(item);
                //}

                if (lastVertices == null)
                {
                    // verticesOnTexture = ConvexHull(verticesOnTexture);
                    ConvexHull(ref verticesOnTexture);

                    SetPixelsInPolygonCS(renderTexture, verticesOnTexture, backgroundColor, computeShader, ref cornersBuffer);
                }
                else
                {
                    foreach (var item in lastVertices)
                    {
                        verticesOnTexture.Add(item);
                    }

                    // verticesOnTexture = ConvexHull(verticesOnTexture);
                    ConvexHull(ref verticesOnTexture);

                    SetPixelsInPolygonCS(renderTexture, verticesOnTexture, backgroundColor, computeShader, ref cornersBuffer);
                }
            }

            /// <summary>
            /// Returns list of pixel coordinates where BoxCollider edges cross the board.
            /// </summary>
            /// <param name="collider">BoxCollider used to get list of coordinates on the board.</param>
            /// <returns>Returns list of pixel coordinates where BoxCollider edges cross the board.</returns>
            private Vector2Int[] GetVerticesOnTexture(BoxCollider collider)
            {
                // List<Vector2Int> verticesOnTexture = new List<Vector2Int>();
                verticesOnTexture.Clear();
                foreach (var item in GetContactPoints(collider, this.transform, corners))
                {
                    Vector2Int posOnTexture = GetPointOnTextureByWorldCoordinates(item, this.transform, corners, resolutionX, resolutionY);
                    verticesOnTexture.Add(posOnTexture);
                }

                verticesOnTexture = OrderVertices(verticesOnTexture);

                return verticesOnTexture.ToArray();
            }

            Vector3[] contactPointsBuffer = new Vector3[DrawingBoardSettings.MaxContactPoints];
            /// <summary>
            /// Returns list of pixel coordinates where BoxCollider edges cross the board.
            /// </summary>
            /// <param name="collider">BoxCollider used to get list of coordinates on the board.</param>
            /// <param name="arr">Array in which result is stored.</param>
            private int GetVerticesOnTexture(BoxCollider collider, ref Vector2Int[] result)
            {
                int c = GetContactPoints(collider, this.transform, corners, ref contactPointsBuffer);
                for (int i = 0; i < c; i++)
                {
                    Vector2Int posOnTexture = GetPointOnTextureByWorldCoordinates(contactPointsBuffer[i], this.transform, corners, resolutionX, resolutionY);
                    result[i] = (posOnTexture);
                }

                return c;
            }

            /// <summary>
            /// Returns list of pixel coordinates where MeshCollider edges cross the board.
            /// </summary>
            /// <param name="meshVerticesInfo">MeshCollider used to get list of coordinates on the board.</param>
            /// <param name="markerTransform">Marker Transform used to convert vertices coordinates from local to global space.</param>
            /// <returns>Returns list of pixel coordinates where MeshCollider edges cross the board.</returns>
            private Vector2Int[] GetVerticesOnTexture(MeshVertexInfo[] meshVerticesInfo, Transform markerTransform)
            {
                verticesOnTexture.Clear();

                foreach (var item in GetContactPoints(meshVerticesInfo, markerTransform, this.transform, corners))
                {
                    Vector2Int posOnTexture = GetPointOnTextureByWorldCoordinates(item, this.transform, corners, resolutionX, resolutionY);
                    verticesOnTexture.Add(posOnTexture);                    
                }

                verticesOnTexture = OrderVertices(verticesOnTexture);

                return verticesOnTexture.ToArray();
            }

            /// <summary>
            /// Returns list of pixel coordinates where eshCollider edges cross the board.
            /// </summary>
            /// <param name="meshVerticesInfo">MeshCollider used to get list of coordinates on the board.</param>
            /// <param name="result">Array in which result is stored.</param>
            private int GetVerticesOnTexture(MeshVertexInfo[] meshVerticesInfo, Transform markerTransform, ref Vector2Int[] result)
            {
                int c = GetContactPoints(meshVerticesInfo, markerTransform, this.transform, corners, ref contactPointsBuffer);
                
                if (c > result.Length)
                {
                    result = new Vector2Int[c];
                }

                int i;
                for (i = 0; i < c; i++)
                {
                    Vector2Int posOnTexture = GetPointOnTextureByWorldCoordinates(contactPointsBuffer[i], this.transform, corners, resolutionX, resolutionY);
                    result[i] = (posOnTexture);
                }

                return i;
            }

            /// <summary>
            /// Draws pixels on the board in given distance to finite line made between position and last position.
            /// </summary>
            /// <param name="position">World position of marker.</param>
            /// <param name="size">Size of marker.</param>
            /// <param name="color">Color of marker.</param>
            /// <param name="lastVertices">World position of marker in last frame. Null by default.</param>
            private void Draw(Vector3 position, float size, Color color, Vector3? lastPosition = null)
            {
                float pixelSize = Vector3.Distance(this.transform.TransformPoint(corners[1]), this.transform.TransformPoint(corners[0])) / resolutionX;

                if (!lastPosition.HasValue)
                {
                    Vector2Int pos1;

                    pos1 = GetPointOnTextureByWorldCoordinates(this.transform.TransformPoint(position), this.transform, corners, resolutionX, resolutionY);

                    DrawingBoardUtils.SetPixelsInRangeCS(renderTexture, pos1, (int)(size / pixelSize) + 1, color, computeShader);
                }
                else
                {
                    Vector2Int pos1, pos2;

                    pos1 = GetPointOnTextureByWorldCoordinates(this.transform.TransformPoint(position), this.transform, corners, resolutionX, resolutionY);
                    pos2 = GetPointOnTextureByWorldCoordinates(this.transform.TransformPoint(lastPosition.Value), this.transform, corners, resolutionX, resolutionY);

                    DrawingBoardUtils.SetPixelsInRangeOnLineCS(renderTexture, pos1, pos2, (int)(size / pixelSize) + 1, color, computeShader);
                }
            }

            /// <summary>
            /// Draws pixels on the board in convex polygon made of currentVertices and lastVertices.
            /// </summary>
            /// <param name="currentVertices">Vertices positions when executing the function.</param>
            /// <param name="color">Color of marker.</param>
            /// <param name="lastVertices">Vertices positions in last frame. Null by default.</param>
            private void Draw(Vector2Int[] currentVertices, Color color, Vector2Int[] lastVertices = null)
            {
                verticesOnTexture.Clear();

                foreach (var item in currentVertices)
                {
                    verticesOnTexture.Add(item);
                }

                if (lastVertices == null)
                {
                    // verticesOnTexture = ConvexHull(verticesOnTexture);
                    ConvexHull(ref verticesOnTexture);

                    SetPixelsInPolygonCS(renderTexture, verticesOnTexture, color, computeShader, ref cornersBuffer);
                }
                else
                {
                    foreach (var item in lastVertices)
                    {
                        verticesOnTexture.Add(item);
                    }

                    // verticesOnTexture = ConvexHull(verticesOnTexture);
                    ConvexHull(ref verticesOnTexture);

                    SetPixelsInPolygonCS(renderTexture, verticesOnTexture, color, computeShader, ref cornersBuffer);
                }
            }

            /// <summary>
            /// Draws pixels on the board in convex polygon made of currentVertices and lastVertices.
            /// </summary>
            /// <param name="currentVertices">Vertices positions when executing the function.</param>
            /// <param name="count">Number of vertices to use.</param>
            /// <param name="color">Color of marker.</param>
            /// <param name="lastVertices">Vertices positions in last frame. Null by default.</param>
            private void Draw(Vector2Int[] currentVertices, int count, Color color, Vector2Int[] lastVertices = null)
            {
                // List<Vector2Int> verticesOnTexture = new List<Vector2Int>();

                //foreach (var item in currentVertices)
                //{
                //    verticesOnTexture.Add(item);
                //}

                verticesOnTexture.Clear();

                for (int i = 0; i < count; i++)
                {
                    verticesOnTexture.Add(currentVertices[i]);
                }

                if (lastVertices == null)
                {
                    // verticesOnTexture = ConvexHull(verticesOnTexture);
                    ConvexHull(ref verticesOnTexture);

                    SetPixelsInPolygonCS(renderTexture, verticesOnTexture, color, computeShader, ref cornersBuffer);
                }
                else
                {
                    foreach (var item in lastVertices)
                    {
                        verticesOnTexture.Add(item);
                    }

                    // verticesOnTexture = ConvexHull(verticesOnTexture);
                    ConvexHull(ref verticesOnTexture);

                    SetPixelsInPolygonCS(renderTexture, verticesOnTexture, color, computeShader, ref cornersBuffer);
                }
            }           

            private void OnDrawGizmos()
            {
                if (!debug)
                {
                    return;
                }

                Gizmos.color = Color.red;

                for (int i = 0; i < corners.Length; i++)
                {
                    Gizmos.DrawWireSphere(this.transform.TransformPoint(corners[i]), 0.01f * i);
                }
            }

            private void OnDestroy()
            {
                renderTexture.Release();
                cornersBuffer.Release();
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(DrawingBoardTexture))]
        class DrawingBoardTextureEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                var drawingBoardTexture = (DrawingBoardTexture)target;
                if (drawingBoardTexture == null) return;

                if (GUILayout.Button("Save Image"))
                {
                    drawingBoardTexture.SaveTextureToFile();
                }

                if (GUILayout.Button("Enable Drawing Switch"))
                {
                    bool isEnabled = drawingBoardTexture.IsDrawingEnabled();
                    drawingBoardTexture.EnableDrawing(!isEnabled);
                }

                if (GUILayout.Button("Clear Board"))
                {
                    drawingBoardTexture.ClearBoard();
                }

                if (GUILayout.Button("Set Image"))
                {
                    drawingBoardTexture.SetRenderTexture(drawingBoardTexture.GetSourceTexture(), true);
                }

                DrawDefaultInspector();
            }
        }
#endif
    }
}