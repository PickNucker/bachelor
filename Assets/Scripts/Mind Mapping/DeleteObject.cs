using UnityEngine;

public class DeleteObject : MonoBehaviour
{

    MultiplayerLineConnector mpl;

    void Awake()
    {
        mpl = FindFirstObjectByType<MultiplayerLineConnector>();
    }

    public void DestroyObject(GameObject obj)
    {
        mpl.DestroyNetworkObject(obj);
    }
}
