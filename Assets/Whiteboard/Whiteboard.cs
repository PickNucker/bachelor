using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class Whiteboard : NetworkBehaviour
{
    [Header("Initial Setup")]
    [SerializeField] private bool useFirstImage = true;
    [SerializeField] private Texture2D firstImage;

    [Header("Textures")]
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Texture2D brushTexture;
    [SerializeField] private Texture2D eraserTexture;

    [Header("Brush Settings")]
    [SerializeField] private Color brushColor = Color.black;
    [SerializeField] private float brushSize = 15.0f;
    [SerializeField] private float smoothSteps = 800f;

    [Header("Eraser Settings")]
    [SerializeField] private GameObject eraserMesh;

    [Header("Marker Settings")]
    private int selectedMarker = 0;
    [SerializeField] private List<Color> colors;
    [SerializeField] private Material baseMarkerMaterial;
    [SerializeField] private List<GameObject> displayMarkers;
    [SerializeField] private GameObject heroMarker;

    [Header("Other Settings")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private GameObject rag;
    [SerializeField] private float hoverOffset = 0.02f;
    [SerializeField] private float markerSmoothingSpeed = 20f;

    private Material drawMaterial;
    private Vector2? previousUV = null;
    private MeshRenderer heroMarkerRenderer;

    // Input System
    private InputAction clickAction;
    private InputAction pointAction;

    private void Awake()
    {
        // Initialize InputActions
       // var inputActions = new InputActionMap("Whiteboard");

        clickAction = new InputAction(type: InputActionType.Button, binding: "<Pointer>/press");
        pointAction = new InputAction(type: InputActionType.Value, binding: "<Pointer>/position");

        clickAction.Enable();
        pointAction.Enable();

    }

    [SerializeField] NearFarInteractor interactor;

    private void Start()
    {
        heroMarkerRenderer = heroMarker.GetComponent<MeshRenderer>();
        InitializeRenderTexture();
        InitializeDisplayMarkers();
        ChangeMarkers();
    }


    private void Update()
    {
        Vector3 endPoint;
        EndPointType resultType = interactor.TryGetCurveEndPoint(out endPoint); // Verwende den Endpunkt vom NearFarInteractor

        // Debug: Ausgabe des resultierenden Endpunkts und des Resultat-Typs
        Debug.Log("EndPoint: " + endPoint + ", ResultType: " + resultType);

        if (resultType != EndPointType.None) // Sicherstellen, dass ein gültiger Endpunkt vorliegt
        {
            Ray ray = new Ray(Camera.main.transform.position, endPoint - Camera.main.transform.position); // Erstelle einen Ray von der Kamera zum Endpunkt
            RaycastHit hit;

            // Debug: Ray und Richtung
            Debug.DrawRay(Camera.main.transform.position, endPoint - Camera.main.transform.position, Color.red);

            if (Physics.Raycast(ray, out hit, interactDistance)) // Raycast zur Interaktion
            {
                // Debug: Ausgabe des Hit-Punkts
                Debug.Log("Raycast Hit Point: " + hit.point);

                if (hit.collider.gameObject == gameObject)
                {
                    if (clickAction.ReadValue<float>() > 0) // Überprüfe, ob der Klick aktiv ist
                    {
                        if (selectedMarker == 5) // Eraser ausgewählt
                        {
                            eraserMesh.SetActive(true);
                            eraserMesh.transform.position = Vector3.Lerp(eraserMesh.transform.position, hit.point, Time.deltaTime * markerSmoothingSpeed);
                        }
                        else
                        {
                            heroMarker.SetActive(true);
                            heroMarker.transform.position = Vector3.Lerp(heroMarker.transform.position, hit.point, Time.deltaTime * markerSmoothingSpeed);
                        }

                        Vector2 uv;
                        if (TryGetUVCoordinates(hit, out uv))
                        {
                            if (previousUV.HasValue)
                            {
                                DrawBetween(previousUV.Value, uv);
                            }
                            else
                            {
                                Draw(uv);
                            }
                            previousUV = uv;
                        }
                    }
                    else
                    {
                        Vector3 markerPosition = hit.point + hit.normal * hoverOffset;
                        if (selectedMarker == 5)
                        {
                            eraserMesh.SetActive(true);
                            eraserMesh.transform.position = Vector3.Lerp(eraserMesh.transform.position, markerPosition, Time.deltaTime * markerSmoothingSpeed);
                        }
                        else
                        {
                            heroMarker.SetActive(true);
                            heroMarker.transform.position = Vector3.Lerp(heroMarker.transform.position, markerPosition, Time.deltaTime * markerSmoothingSpeed);
                        }
                        previousUV = null;
                    }
                }
                else
                {
                    heroMarker.SetActive(false);
                    eraserMesh.SetActive(false);
                }

                // Wechsel der Marker (falls notwendig)
                for (int i = 0; i < displayMarkers.Count; i++)
                {
                    if (clickAction.triggered && hit.collider.gameObject == displayMarkers[i])
                    {
                        ChangeMarkers(i);
                        break;
                    }
                }

                if (clickAction.triggered && hit.collider.gameObject == rag)
                {
                    ClearRenderTexture();
                }
            }
            else
            {
                previousUV = null;
                heroMarker.SetActive(false);
                eraserMesh.SetActive(false);
            }
        }
        else
        {
            previousUV = null;
            heroMarker.SetActive(false);
            eraserMesh.SetActive(false);
        }
    }



    void ChangeMarkers(int index = 0)
    {
        SubmitMarkerChangeServerRpc(index);
    }

    [ServerRpc]
    void SubmitMarkerChangeServerRpc(int markerIndex)
    {
        selectedMarker = markerIndex;
        ChangeMarkerOnClientsClientRpc(markerIndex);
    }

    [ClientRpc]
    void ChangeMarkerOnClientsClientRpc(int markerIndex)
    {
        selectedMarker = markerIndex;
        for (int i = 0; i < displayMarkers.Count; i++)
        {
            displayMarkers[i].SetActive(i != selectedMarker);
        }
        UpdateBrushSettings();
    }

    private void UpdateBrushSettings()
    {
        if (selectedMarker == 5) // Eraser
        {
            brushColor = Color.white;
            brushSize = 150f;
            drawMaterial.SetTexture("_MainTex", eraserTexture);

            heroMarker.SetActive(false);
            eraserMesh.SetActive(true);
        }
        else
        {
            brushColor = colors[selectedMarker];
            brushSize = 15f;
            drawMaterial.SetTexture("_MainTex", brushTexture);

            Material markerMaterialInstance = new Material(baseMarkerMaterial)
            {
                color = colors[selectedMarker]
            };
            heroMarkerRenderer.material = markerMaterialInstance;

            heroMarker.SetActive(true);
            eraserMesh.SetActive(false);
        }
    }

    private void HandleClick(RaycastHit hit)
    {
        Vector2 uv;
        if (TryGetUVCoordinates(hit, out uv))
        {
            if (previousUV.HasValue)
            {
                SubmitDrawRequestServerRpc(previousUV.Value, uv, brushColor, brushSize);
            }
            else
            {
                SubmitDrawRequestServerRpc(uv, uv, brushColor, brushSize);
            }
            previousUV = uv;
        }
    }

    [ServerRpc]
    void SubmitDrawRequestServerRpc(Vector2 startUV, Vector2 endUV, Color color, float size)
    {
        DrawOnClientsClientRpc(startUV, endUV, color, size);
    }

    [ClientRpc]
    void DrawOnClientsClientRpc(Vector2 startUV, Vector2 endUV, Color color, float size)
    {
        brushColor = color;
        brushSize = size;

        if (startUV != endUV)
        {
            DrawBetween(startUV, endUV);
        }
        else
        {
            Draw(startUV);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestOwnershipServerRpc(ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        NetworkObject.ChangeOwnership(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void SyncRenderTextureServerRpc()
    {
        // RenderTexture-Synchronisation bei neuen Spielern
        // (hier können zusätzliche Daten synchronisiert werden, falls benötigt)
    }

    private void HandleHover(RaycastHit hit)
    {
        Vector3 markerPosition = hit.point + hit.normal * hoverOffset;
        if (selectedMarker == 5)
        {
            eraserMesh.SetActive(true);
            eraserMesh.transform.position = Vector3.Lerp(eraserMesh.transform.position, markerPosition, Time.deltaTime * markerSmoothingSpeed);
        }
        else
        {
            heroMarker.SetActive(true);
            heroMarker.transform.position = Vector3.Lerp(heroMarker.transform.position, markerPosition, Time.deltaTime * markerSmoothingSpeed);
        }
        previousUV = null;
    }

    void InitializeRenderTexture()
    {
        RenderTexture.active = renderTexture;

        if (useFirstImage)
        {
            if (firstImage != null)
            {
                Graphics.Blit(firstImage, renderTexture);
            }
            else
            {
                GL.Clear(true, true, Color.white);
            }

            useFirstImage = false;
        }

        RenderTexture.active = null;

        Shader drawShader = Shader.Find("Custom/DrawShader");
        drawMaterial = new Material(drawShader);
        drawMaterial.SetTexture("_MainTex", brushTexture);
    }

    void InitializeDisplayMarkers()
    {
        for (int i = 0; i < displayMarkers.Count; i++)
        {
            if (i != 5)
            {
                MeshRenderer renderer = displayMarkers[i].GetComponent<MeshRenderer>();
                Material markerMaterialInstance = new Material(baseMarkerMaterial)
                {
                    color = colors[i]
                };
                renderer.material = markerMaterialInstance;
            }
        }
    }

    void ClearRenderTexture()
    {
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.white);
        RenderTexture.active = null;
    }

    bool TryGetUVCoordinates(RaycastHit hit, out Vector2 uv)
    {
        MeshCollider meshCollider = hit.collider as MeshCollider;
        uv = Vector2.zero;

        if (meshCollider == null || meshCollider.sharedMesh == null)
        {
            return false;
        }

        Mesh mesh = meshCollider.sharedMesh;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;

        int triangleIndex = hit.triangleIndex * 3;
        Vector3 p0 = vertices[triangles[triangleIndex + 0]];
        Vector3 p1 = vertices[triangles[triangleIndex + 1]];
        Vector3 p2 = vertices[triangles[triangleIndex + 2]];

        Vector2 uv0 = uvs[triangles[triangleIndex + 0]];
        Vector2 uv1 = uvs[triangles[triangleIndex + 1]];
        Vector2 uv2 = uvs[triangles[triangleIndex + 2]];

        Vector3 barycentric = hit.barycentricCoordinate;
        uv = uv0 * barycentric.x + uv1 * barycentric.y + uv2 * barycentric.z;

        return true;
    }

    void Draw(Vector2 textureCoord)
    {
        RenderTexture.active = renderTexture;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, renderTexture.width, renderTexture.height, 0);

        Vector2 brushPos = new Vector2(textureCoord.x * renderTexture.width, (1 - textureCoord.y) * renderTexture.height);

        drawMaterial.SetColor("_Color", brushColor);
        drawMaterial.SetPass(0);

        Graphics.DrawTexture(new Rect(brushPos.x - brushSize / 2, brushPos.y - brushSize / 2, brushSize, brushSize), drawMaterial.GetTexture("_MainTex"), drawMaterial);

        GL.PopMatrix();
        RenderTexture.active = null;
    }

    void DrawBetween(Vector2 start, Vector2 end)
    {
        int steps = Mathf.CeilToInt(Vector2.Distance(start, end) * smoothSteps);
        for (int i = 0; i <= steps; i++)
        {
            Vector2 textureCoord = Vector2.Lerp(start, end, (float)i / steps);
            Draw(textureCoord);
        }
    }

}
