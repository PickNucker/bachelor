using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ApplyText : NetworkBehaviour
{
    [SerializeField] private TMP_InputField inputField; // Referenz zum InputField
    [SerializeField] public TMP_Text displayText;      // Referenz zum Text-Objekt

    private void Start()
    {
        if (inputField != null)
        {
            // Füge Listener für OnEndEdit hinzu
            //inputField.onValueChanged.AddListener(OnInputFieldEndEdit);
        }
    }

   // private override void OnDestroy()
   // {
   //     base.OnDestroy();
   //     if (inputField != null)
   //     {
   //         //inputField.onValueChanged.RemoveListener(OnInputFieldEndEdit);
   //     }
   // }

    /// <summary>
    /// Wird aufgerufen, wenn die Eingabe im InputField abgeschlossen ist.
    /// </summary>
    /// <param name="text">Der eingegebene Text.</param>
    public void OnInputFieldEndEdit()
    {

       //if (string.IsNullOrEmpty(text))
       //{
       //    Debug.LogWarning("Eingabetext ist leer. Text wird nicht synchronisiert.");
       //
       //
       //}
        
        string text = inputField.text;

        if (IsServer)
        {
            // Server synchronisiert direkt mit allen Clients
            UpdateTextServerRpc(text);
            UpdateTextOnClientRpc(text);
        }
        else
        {
            // Client informiert den Server
            UpdateTextServerRpc(text);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateTextServerRpc(string text)
    {
        // Server empfängt Text vom Client und synchronisiert mit allen Clients
        UpdateTextOnClientRpc(text);
    }

    [ClientRpc]
    private void UpdateTextOnClientRpc(string text)
    {
        // Aktualisiere das Textfeld auf allen Clients
        ApplyTextTo(text);
    }

    private void ApplyTextTo(string text)
    {
        if (displayText != null)
        {
            displayText.text = text;
            Debug.Log($"Text geändert auf: {text}");
        }
        else
        {
            Debug.LogWarning("DisplayText ist nicht gesetzt!");
        }
    }
}
