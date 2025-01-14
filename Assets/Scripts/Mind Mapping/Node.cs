using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class Node : NetworkBehaviour
{
    [SerializeField] private GameObject firstObject;
    [SerializeField] private GameObject secondObject;
    [SerializeField] private List<GameObject> spawnedLineInstanz = new List<GameObject>(); // Alle verbundenen Linien
    [SerializeField] private GameObject spawnedLineInstanzUI;
    [SerializeField] private GameObject parentSpawnPos;
    [SerializeField] private GameObject lineConnectionShowcasePrefab;

    public MultiplayerLineConnector connector;

    private void OnDestroy()
    {
        base.OnDestroy();

        // Lösche alle Verbindungen (Server-seitig oder per RPC)
        Destroying();
    }

    public void Destroying()
    {
        if (spawnedLineInstanz.Count > 0)
        {
            if (IsServer)
            {
                // Alle verbundenen Linien direkt despawnen
                foreach (GameObject line in spawnedLineInstanz)
                {
                    if (line == null) continue;
                    line.GetComponent<NetworkObject>().Despawn();
                }

                // UI-Verbindung löschen
                if (spawnedLineInstanzUI != null)
                {
                    spawnedLineInstanzUI.GetComponent<NetworkObject>().Despawn();
                }
            }
            else
            {
                // Über RPC an den Server delegieren
                foreach (GameObject line in spawnedLineInstanz)
                {
                    if (line == null) continue;
                    DespawnSpawnedLineServerRpc(line.GetComponent<NetworkObject>().NetworkObjectId);
                }

                // UI-Verbindung löschen
                if (spawnedLineInstanzUI != null)
                {
                    DespawnSpawnedLineServerRpc(spawnedLineInstanzUI.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
        }

        // Verbindungsliste leeren
        spawnedLineInstanz.Clear();
    }

    public void InitializeConnectorList(GameObject first, GameObject second, GameObject line)
    {
        firstObject = first;
        secondObject = second;
        spawnedLineInstanz.Add(line); // Linie hinzufügen
        //this.connector = connector;

        SpawnLineConnectionUIServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnLineConnectionUIServerRpc()
    {
        if (lineConnectionShowcasePrefab == null || parentSpawnPos == null)
        {
            Debug.LogWarning("lineConnectionShowcasePrefab oder parentSpawnPos ist nicht gesetzt!");
            return;
        }

       // // Instanziere die UI-Linie auf dem Server
       // GameObject lineShowcaseInstance = Instantiate(lineConnectionShowcasePrefab, parentSpawnPos.transform);
       //
       // // Netzwerk-Komponente prüfen und spawnen
       // NetworkObject networkObject = lineShowcaseInstance.GetComponent<NetworkObject>();
       // if (networkObject == null)
       // {
       //     networkObject = lineShowcaseInstance.AddComponent<NetworkObject>();
       // }
       // networkObject.Spawn();
       //
        //spawnedLineInstanzUI = lineShowcaseInstance;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnSpawnedLineServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            networkObject.Despawn();
        }
        else
        {
            Debug.LogWarning($"Netzwerkobjekt mit ID {networkObjectId} konnte nicht gefunden werden!");
        }
    }
}
