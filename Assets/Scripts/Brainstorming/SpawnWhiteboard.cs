using Unity.Netcode;
using UnityEngine;

public class SpawnWhiteboard : NetworkBehaviour
{
    [SerializeField] private GameObject whiteboardPrefab;
    [SerializeField] private Transform whiteboardSpawnPos;

    // Diese Methode wird vom Client aufgerufen
    public void Spawn()
    {
        // ServerRPC aufrufen, um das Whiteboard auf dem Server zu spawnen
        SpawnNodeServerRpc();
    }

    // ServerRPC, um das Whiteboard auf dem Server zu spawnen
    [ServerRpc(RequireOwnership = false)]
    private void SpawnNodeServerRpc()
    {
        if (whiteboardSpawnPos != null)
        {
            // Position und Rotation vom Spawnpunkt holen
            Vector3 position = whiteboardSpawnPos.position;
            Quaternion rotation = whiteboardSpawnPos.rotation;

            GameObject wb = Instantiate(whiteboardPrefab, position, rotation);
            var networkObject = wb.GetComponent<NetworkObject>();
            if (networkObject != null)
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
            Debug.LogWarning("Whiteboard Spawn Position ist nicht definiert.");
        }
    }
}
