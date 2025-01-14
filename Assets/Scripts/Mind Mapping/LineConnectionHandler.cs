using Unity.Netcode;
using UnityEngine;

public class LineConnectionHandler : NetworkBehaviour
{
    [SerializeField] Transform parentSpawnPos;
    [SerializeField] GameObject lineConnectionShowcasePrefab;

    [SerializeField] GameObject firstObject;
    [SerializeField] GameObject seconObject;

    [SerializeField] LineRenderer lr_connection;

    MultiplayerLineConnector mlc;

    bool lineConnectionRealised = false;

    void Start()
    {
        mlc = FindFirstObjectByType<MultiplayerLineConnector>();
    }

    void Update()
    {
        if (lineConnectionRealised)
        {
            if(firstObject == null || seconObject == null)
            {
                lineConnectionRealised=false;
               // DeleteConnection();
            }
        }
    }

    public void InitializeLineUI(Transform firstObj, Transform secondObj)
    {

    }

   // [ServerRpc(RequireOwnership = false)]
   // public void DeleteConnection()
   // {
   //     if(lr_connection == null)
   //     {
   //
   //         Debug.LogWarning("LR Connection war nicht null");
   //     }
   //     else
   //     {
   //         //lr_connection
   //     }
   //
   // }
   //
    
}
