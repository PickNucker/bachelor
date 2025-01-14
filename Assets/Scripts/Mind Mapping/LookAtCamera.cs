using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] Camera cam;

    private void OnEnable()
    {
        cam = Camera.main;
    }

    void Update()
    {
        transform.LookAt(cam.transform.localPosition);
    }
}
