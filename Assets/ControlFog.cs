using UnityEngine;

public class ControlFog : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddFog()
    {
        RenderSettings.fogDensity = 0.15f;
    }
    public void ReeduceFog()
    {
        RenderSettings.fogDensity = 0.01f;
    }
}
