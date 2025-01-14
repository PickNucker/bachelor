using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

[CanSelectMultiple(false)]
public class MultiplayerLineConnector : NetworkBehaviour
{
    [Header("Node")]
    [SerializeField] GameObject nodePrefab;
    [SerializeField] Transform nodeSpawnPosition;
    [SerializeField] GameObject playerMenu;

    private bool doubleClicked = false;
    private Transform lastClickedObject = null;
    private float doubleClickTime = 0.3f;
    private Coroutine doubleClickCoroutine;

    [Header("Line Renderer Settings")]
    [SerializeField] private LineRenderer lineRendererPrefab;

    [Header("Input Actions")]
    [SerializeField] private InputActionProperty drawLineAction;

    private LineRenderer currentLineRenderer;
    public Transform firstSelectedObject;
    public Transform secondObject;
    private bool isMarking = false;

    private LineRenderer activeLineRenderer;

    private Dictionary<string, (Transform, Transform)> withLineCombinedNodes;

    GameObject spawnedLineInstance;

    private void Awake()
    {
        withLineCombinedNodes = new Dictionary<string, (Transform, Transform)>();
    }

    public void OnDrawLinePressed(Transform grabableTransform, GameObject menu)
    {
        // Überprüfen, ob dasselbe Objekt angeklickt wurde
        if (lastClickedObject == grabableTransform && doubleClicked)
        {
            // Doppelklick erkannt: Öffne Menü
            OpenPlayerMenu(menu);
            ResetState();
        }
        else
        {
            // Wenn kein Doppelklick, normal verarbeiten
            if (!isMarking)
            {
                MarkObject(grabableTransform);
            }
            else
            {
                ConnectToObject(grabableTransform);
            }

            // Aktualisiere das zuletzt angeklickte Objekt und starte die Doppelklick-Prüfung
            lastClickedObject = grabableTransform;

            if (doubleClickCoroutine != null)
            {
                StopCoroutine(doubleClickCoroutine);
            }
            doubleClickCoroutine = StartCoroutine(GetIfDoubleClicked());
        }
    }

    private IEnumerator GetIfDoubleClicked()
    {
        doubleClicked = true;
        yield return new WaitForSeconds(doubleClickTime);
        doubleClicked = false;
    }

    private void MarkObject(Transform selected)
    {
        if (selected != null)
        {
            firstSelectedObject = selected;
            isMarking = true;
        }
        else
        {
            Debug.LogWarning("Kein gültiges Objekt gefunden, das markiert werden kann.");
        }
    }

    private void ConnectToObject(Transform selected)
    {
        if (selected != null && firstSelectedObject != null)
        {
            secondObject = selected;

            if (ConnectionExists(firstSelectedObject, secondObject))
            {
                Debug.Log("Die Verbindung existiert bereits.");
                CancelLine();
                return;
            }

            // Verbindung erstellen
            AddConnection(firstSelectedObject, secondObject);
            CompleteLine(secondObject.position);

            if (IsServer)
            {
                SaveConnectionServerRpc(firstSelectedObject.GetComponent<NetworkObject>().NetworkObjectId,
                                        secondObject.GetComponent<NetworkObject>().NetworkObjectId);
            }
            else
            {
                Debug.LogWarning("Dieser Client kann keine Server-RPCs senden.");
                // Client sendet ClientRpc, um Linie zu zeichnen
                //  SaveConnectionClientRpc(firstSelectedObject.GetComponent<NetworkObject>().NetworkObjectId,
                //                           secondObject.GetComponent<NetworkObject>().NetworkObjectId);
                RequestConnection(firstSelectedObject,
                                           secondObject);
            }

            ResetState();
        }
        else
        {
            Debug.LogWarning("Kein gültiges zweites Objekt gefunden oder das gleiche Objekt wurde gewählt.");
            CancelLine();
        }
    }

    private void CompleteLine(Vector3 endPosition)
    {
        if (currentLineRenderer == null)
        {
            currentLineRenderer = Instantiate(lineRendererPrefab);
        }

        currentLineRenderer.positionCount = 2;
        currentLineRenderer.SetPosition(0, firstSelectedObject.position);
        currentLineRenderer.SetPosition(1, endPosition);

        activeLineRenderer = currentLineRenderer;

        // Startet die Aktualisierung der Linie
        if (firstSelectedObject != null && secondObject != null)
        {
            StartCoroutine(UpdateLinePosition());
        }

        Destroy(currentLineRenderer.gameObject);
        currentLineRenderer = null;
    }

    private bool AddConnection(Transform firstObject, Transform secondObject)
    {
        string key = GenerateKey(firstObject, secondObject);

        if (withLineCombinedNodes.ContainsKey(key))
        {
            return false;
        }

        withLineCombinedNodes[key] = (firstObject, secondObject);
        return true;
    }

    private bool ConnectionExists(Transform firstObject, Transform secondObject)
    {
        string key = GenerateKey(firstObject, secondObject);
        return withLineCombinedNodes.ContainsKey(key);
    }

    private string GenerateKey(Transform a, Transform b)
    {
        if (a == null || b == null) return "";
        int id1 = a.GetInstanceID();
        int id2 = b.GetInstanceID();

        return id1 < id2 ? $"{id1}-{id2}" : $"{id2}-{id1}";
    }

    private void CancelLine()
    {
        if (currentLineRenderer != null)
        {
            Destroy(currentLineRenderer.gameObject);
            currentLineRenderer = null;
        }
        ResetState();
    }

    private void ResetState()
    {
        firstSelectedObject = null;
        secondObject = null;
        isMarking = false;
    }

    private IEnumerator UpdateLinePosition()
    {
        while (activeLineRenderer != null && firstSelectedObject != null && secondObject != null)
        {
            activeLineRenderer.SetPosition(0, firstSelectedObject.position);
            activeLineRenderer.SetPosition(1, secondObject.position);
            yield return null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SaveConnectionServerRpc(ulong firstNodeId, ulong secondNodeId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(firstNodeId, out NetworkObject firstNode) ||
            !NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(secondNodeId, out NetworkObject secondNode))
        {
            Debug.LogError("Knoten wurden nicht gefunden. Abbruch der Verbindungserstellung.");
            return;
        }

        GameObject lineInstance = Instantiate(lineRendererPrefab.gameObject);
        NetworkObject lineNetworkObject = lineInstance.GetComponent<NetworkObject>();

        if (lineNetworkObject != null)
        {
            lineNetworkObject.Spawn(); // Spawn Linie
            firstNode.GetComponent<Node>().InitializeConnectorList(firstNode.gameObject, secondNode.gameObject, lineInstance);
            secondNode.GetComponent<Node>().InitializeConnectorList(secondNode.gameObject, firstNode.gameObject, lineInstance);
            MultiplayerLineUpdater updater = lineInstance.AddComponent<MultiplayerLineUpdater>();
            updater.Initialize(firstNode, secondNode);

            // RPC aufrufen
            UpdateLineOnClientsClientRpc(lineNetworkObject.NetworkObjectId, firstNodeId, secondNodeId);
        }
        else
        {
            Debug.LogError("Netzwerkobjekt konnte nicht gefunden werden.");
        }
    }


    [ClientRpc]
    private void UpdateLineOnClientsClientRpc(ulong lineObjectId, ulong firstNodeId, ulong secondNodeId)
    {
        StartCoroutine(WaitForNetworkObjectsAndInitialize(lineObjectId, firstNodeId, secondNodeId));

        Debug.Log($"UpdateLineOnClientsClientRpc: lineObjectId={lineObjectId}");
    }

    private IEnumerator WaitForNetworkObjectsAndInitialize(ulong lineObjectId, ulong firstNodeId, ulong secondNodeId)
    {
        NetworkObject lineObject = null, firstNode = null, secondNode = null;

        while (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(lineObjectId, out lineObject) ||
               !NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(firstNodeId, out firstNode) ||
               !NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(secondNodeId, out secondNode))
        {
            yield return null; // Warte einen Frame
        }

        MultiplayerLineUpdater updater = lineObject.GetComponent<MultiplayerLineUpdater>();
        if (updater != null)
        {
            updater.Initialize(firstNode, secondNode);
        }
        else
        {
            Debug.LogError("MultiplayerLineUpdater konnte auf dem Linienobjekt nicht gefunden werden.");
        }
    }



    public void RequestConnection(Transform firstObject, Transform secondObject)
    {
        if (firstObject.TryGetComponent<NetworkObject>(out var firstNetworkObject) &&
            secondObject.TryGetComponent<NetworkObject>(out var secondNetworkObject))
        {
            RequestConnectionServerRpc(firstNetworkObject.NetworkObjectId, secondNetworkObject.NetworkObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestConnectionServerRpc(ulong firstNodeId, ulong secondNodeId)
    {
        SaveConnectionServerRpc(firstNodeId, secondNodeId);
    }

    public void Spawn()
    {
        SpawnNodeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnNodeServerRpc()
    {
        if (nodeSpawnPosition != null)
        {
            Vector3 position = nodeSpawnPosition.position;
            Quaternion rotation = nodeSpawnPosition.rotation;

            GameObject node = Instantiate(nodePrefab, position, rotation);

            if (node.TryGetComponent<NetworkObject>(out var networkObject))
            {
                networkObject.Spawn();
            }
            else
            {
                Debug.LogWarning("Das Prefab hat keine NetworkObject-Komponente.");
            }
        }
        else
        {
            Debug.LogWarning("nodeSpawnPosition ist nicht definiert.");
        }
    }

    public void DestroyNetworkObject(GameObject objectToDestroy)
    {
        // Hole die NetworkObject-Komponente
        if (objectToDestroy.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            if (IsServerOrHost())
            {
                // Direkt auf dem Server despawnen
                DespawnObject(networkObject);
            }
            else
            {
                // Auf dem Client: Sende die NetworkObjectId zum Server
                DestroyObjectServerRpc(networkObject.NetworkObjectId);
            }
        }
        else
        {
            Debug.LogWarning("Das Objekt hat keine NetworkObject-Komponente!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyObjectServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            Debug.Log("Das Objekt wird vom Server despawned.");
            DespawnObject(networkObject);
        }
        else
        {
            Debug.LogWarning($"NetworkObject mit ID {networkObjectId} konnte nicht gefunden werden!");
        }
    }


    private void DespawnObject(NetworkObject networkObject)
    {
        if (networkObject != null)
        {
            Debug.Log($"NetworkObject '{networkObject.name}' wird despawned.");
            networkObject.Despawn(true);
        }
        else
        {
            Debug.LogWarning("NetworkObject ist null und kann nicht despawned werden!");
        }
    }

    private bool IsServerOrHost()
    {
        return NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost);
    }

    private void OpenPlayerMenu(GameObject menu)
    {
        menu.SetActive(true);
    }
}





