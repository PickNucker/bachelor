using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RechterCon : MonoBehaviour
{
    [SerializeField] NearFarInteractor rechterCon;

    public NearFarInteractor GetRechterController() => rechterCon;
}
