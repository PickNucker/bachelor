using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Mindmapping : MonoBehaviour
{

    [SerializeField] Transform nodePivot;
    [SerializeField] Transform node2Pivot;
    [SerializeField] LineRenderer lineRendererPrefab;

    [SerializeField] Transform meshTargetObject;
    [SerializeField] Slider sizeSlider;
    [SerializeField] Mesh cubeMesh;
    [SerializeField] Mesh sphereMesh;
    [SerializeField] Mesh triangleMesh;
    [SerializeField] Renderer objectRenderer;
    [SerializeField] TMP_Text textElement;

    private MeshFilter meshFilter;

    LineRenderer lr;

    bool lrCreated;

    void Start()
    {
        sizeSlider.value = 0.2f;
        sizeSlider.onValueChanged.AddListener(UpdateSize);
        meshFilter = GetComponent<MeshFilter>();
    }

    void Update()
    {

       // if (Mouse.current.leftButton.wasPressedThisFrame)
       // {
       //     //lr = CreateLineBetweenPoints(nodePivot);
       //     lrCreated = true;
       //
       // }
       // else if(Mouse.current.leftButton.wasReleasedThisFrame)
       // {
       //     if (lrCreated)
       //     {
       //         //ConnectLineToPoint(lr, node2Pivot);
       //     }
       //     else
       //     {
       //         Destroy(lr);
       //         lr = null;
       //     }
       // }
       //
       // if (Mouse.current.leftButton.isPressed)
       // {
       //     if(lr != null)
       //     {
       //         lr.SetPosition(1, Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
       //         Debug.Log("isPressing");
       //     }
       // }
    }

    public void CreateLineBetweenPoints(Transform spawnPosition)
    {
        LineRenderer newInstantiatedLr = Instantiate(lineRendererPrefab, spawnPosition.position, Quaternion.identity);
        newInstantiatedLr.positionCount = 2;
        newInstantiatedLr.SetPosition(0, spawnPosition.position);
        lr = newInstantiatedLr;
        //return newInstantiatedLr;
    }

    public void Debuging()
    {
        Debug.Log("Clicked Object: " + this.gameObject);
    }

    void ConnectLineToPoint(LineRenderer newLr, Transform secondNode)
    {
        newLr.SetPosition(1, secondNode.position);       
    }

    public void SetLineColor(LineRenderer lineRenderer, Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    public void UpdateSize(float value)
    {
        float scaleValue = Mathf.Lerp(0.05f, 0.4f, value);
        meshTargetObject.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
    }

    public void SetShapeToCube()
    {
        meshFilter.mesh = cubeMesh;
    }
    public void SetShapeToSphere()
    {
        meshFilter.mesh = sphereMesh;
    }
    public void SetShapeToTriangle()
    {
        meshFilter.mesh = triangleMesh;
    }
    public void SetColor(Color color)
    {
        objectRenderer.material.color = color;
    }
    public void SetText(string newText)
    {
        textElement.text = newText;
    }
}
