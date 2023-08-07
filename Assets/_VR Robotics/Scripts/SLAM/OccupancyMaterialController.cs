using UnityEngine;

public class OccupancyMaterialController : MonoBehaviour
{
    [SerializeField]
    Material occupancyMaterial;
    [SerializeField, Tooltip("Only set if you want to restrict visibility of the map.")]
    Transform centreTransform;

    OccupancyMeshGenerator meshGenerator;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().material = occupancyMaterial;
        meshGenerator = OccupancyMeshGenerator.GetOrCreateInstance();
        occupancyMaterial.mainTexture = meshGenerator.OccupancyTexture;
    }

    // Update is called once per frame
    void Update()
    {
        if (centreTransform != null)
        {
            occupancyMaterial.SetVector("_Centre", centreTransform.position);
            occupancyMaterial.SetFloat("_Rotation", -centreTransform.GetComponentInParent<Transform>().rotation.eulerAngles.x);
        }
    }
}
