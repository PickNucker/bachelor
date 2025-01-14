using Unity.Netcode;
using UnityEngine;

public class MultiplayerLineUpdater : NetworkBehaviour
{
    private NetworkObject startNetworkObject;
    private NetworkObject endNetworkObject;
    private LineRenderer lineRenderer;

    public void Initialize(NetworkObject start, NetworkObject end)
    {
        startNetworkObject = start;
        endNetworkObject = end;
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer fehlt auf dem Objekt.");
        }
        else
        {
            Debug.Log($"LineRenderer erfolgreich initialisiert zwischen {startNetworkObject.name} und {endNetworkObject.name}");
        }
    }


    private void Update()
    {
        if (startNetworkObject != null && endNetworkObject != null && lineRenderer != null)
        {
            Transform startTransform = startNetworkObject.transform;
            Transform endTransform = endNetworkObject.transform;

            lineRenderer.SetPosition(0, startTransform.position);
            lineRenderer.SetPosition(1, endTransform.position);
        }
    }
}