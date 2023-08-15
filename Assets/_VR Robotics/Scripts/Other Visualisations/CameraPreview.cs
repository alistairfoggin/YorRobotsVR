using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CameraPreview : XRBaseInteractable
{
    [SerializeField]
    private MeshRenderer m_FrontScreen;
    [SerializeField]
    private Transform m_TrackingOffsetTransform;

    private CameraSubscriber m_Subscriber;
    private OccupancyMeshGenerator m_MeshGenerateor;
    private MeshRenderer m_OverheadScreen;

    bool m_IsCameraTop = true;

    // Start is called before the first frame update
    void Start()
    {
        m_Subscriber = CameraSubscriber.GetOrCreateInstance();
        m_MeshGenerateor = OccupancyMeshGenerator.GetOrCreateInstance();
        m_MeshGenerateor.transformUpdateDelegate += UpdatePosition;

        m_OverheadScreen = GetComponent<MeshRenderer>();
        m_OverheadScreen.material.mainTexture = m_Subscriber.ImageTexture;
        m_FrontScreen.gameObject.SetActive(false);
        m_FrontScreen.material.mainTexture = m_Subscriber.ImageTexture;
        m_TrackingOffsetTransform.gameObject.SetActive(false);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (m_IsCameraTop)
        {
            m_FrontScreen.gameObject.SetActive(true);
            m_OverheadScreen.material.mainTexture = m_MeshGenerateor.OccupancyTexture;
            if (m_MeshGenerateor.OccupancyTexture.width > m_MeshGenerateor.OccupancyTexture.height)
            {
                transform.localScale = new Vector3((float) m_MeshGenerateor.OccupancyTexture.width / m_MeshGenerateor.OccupancyTexture.height, 1f, 1f);
            }
            else
            {
                transform.localScale = new Vector3(1f, 1f, (float) m_MeshGenerateor.OccupancyTexture.height / m_MeshGenerateor.OccupancyTexture.width);
            }
            m_TrackingOffsetTransform.gameObject.SetActive(true);
        }
        else
        {
            m_FrontScreen.gameObject.SetActive(false);
            m_OverheadScreen.material.mainTexture = m_Subscriber.ImageTexture;
            transform.localScale = Vector3.one;
            m_TrackingOffsetTransform.gameObject.SetActive(false);
        }
        m_IsCameraTop = !m_IsCameraTop;
    }

    void UpdatePosition(Vector3 localPosition, Quaternion localRotation)
    {
        if (!m_IsCameraTop)
        {
            if (m_MeshGenerateor.OccupancyTexture.width > m_MeshGenerateor.OccupancyTexture.height)
            {
                transform.localScale = new Vector3((float)m_MeshGenerateor.OccupancyTexture.width / m_MeshGenerateor.OccupancyTexture.height, 1f, 1f);
            }
            else
            {
                transform.localScale = new Vector3(1f, 1f, (float)m_MeshGenerateor.OccupancyTexture.height / m_MeshGenerateor.OccupancyTexture.width);
            }
        }

        print(localPosition);
        m_TrackingOffsetTransform.localPosition = new Vector3(-localPosition.z + transform.localScale.x, 0, -localPosition.x + transform.localScale.z);
        m_TrackingOffsetTransform.localRotation = localRotation;
    }
}
