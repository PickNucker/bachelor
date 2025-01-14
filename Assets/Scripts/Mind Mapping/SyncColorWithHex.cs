using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SyncMaterialColor : NetworkBehaviour
{
    [SerializeField] private Material baseMaterial;
    [SerializeField] List<MeshRenderer> objectRenderer = new List<MeshRenderer>();

    [SerializeField] Material objectMaterial;


    private void Awake()
    {
        CreateAndAssignMaterial();
    }

    // Normale Methode für Farbänderung
    public void ChangeBaseColor(string hexColor)
    {
        if (IsServer)
        {
            // Server setzt Farbe direkt und synchronisiert
            SetBaseColor(hexColor);
            ChangeBaseColorClientRpc(hexColor);
        }
        else
        {
            // Client fordert Server an
            ChangeBaseColorServerRpc(hexColor);
        }
    }

    // ServerRpc für Clients, um Farbänderung anzufordern
    [ServerRpc(RequireOwnership = false)]
    public void ChangeBaseColorServerRpc(string hexColor)
    {
        // Server setzt Farbe und synchronisiert mit allen Clients
        SetBaseColor(hexColor);
        ChangeBaseColorClientRpc(hexColor);
    }

    // ClientRpc zur Synchronisierung auf allen Clients
    [ClientRpc]
    public void ChangeBaseColorClientRpc(string hexColor)
    {
        SetBaseColor(hexColor);
    }

    // Hilfsmethode: Basisfarbe setzen
    private void SetBaseColor(string hexColor)
    {
        if (ColorUtility.TryParseHtmlString($"#{hexColor}", out Color color))
        {
            objectMaterial.SetColor("_BaseColor", color);
        }
        else
        {
            Debug.LogError($"Ungültiger Hexadezimal-Farbcode: {hexColor}");
        }
    }

    //[ServerRpc(RequireOwnership = false)]
    private void CreateAndAssignMaterial()
    {
        //objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            objectMaterial = new Material(baseMaterial);

            foreach(MeshRenderer renderer in objectRenderer)
            {
                renderer.material = objectMaterial;
            }
        }
        else
        {
            Debug.LogWarning("Kein Renderer gefunden. Material konnte nicht gesetzt werden.");
        }
    }
}


