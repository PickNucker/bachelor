using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AttachLineConnector : MonoBehaviour
{
    public MultiplayerLineConnector attachScript;
    public GameObject menu;

    private void OnEnable()
    {
        attachScript = FindFirstObjectByType<MultiplayerLineConnector>();

    }

    public void StartDrawLine()
    {
        attachScript.OnDrawLinePressed(this.transform, menu);
    }
}
