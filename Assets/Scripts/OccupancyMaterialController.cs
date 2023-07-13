using UnityEngine;

public class OccupancyMaterialController : MonoBehaviour
{
    [SerializeField]
    Material shaderBoundsMaterial;
    [SerializeField]
    Transform centreTransform;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().material = shaderBoundsMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        shaderBoundsMaterial.SetVector("_Centre", new Vector2(centreTransform.position.x, centreTransform.position.z));
    }
}
