using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CameraPreview : XRBaseInteractable
{
    [SerializeField]
    private MeshRenderer m_FrontScreen;
    [SerializeField]
    private Transform m_OverheadMapTransform;

    private CameraSubscriber m_Subscriber;
    private MeshRenderer m_OverheadScreen;
    private MeshCollider m_OverheadScreenCollider;

    private bool m_IsCameraTop = true;

    // Start is called before the first frame update
    void Start()
    {
        m_Subscriber = CameraSubscriber.GetOrCreateInstance();

        m_OverheadScreen = GetComponent<MeshRenderer>();
        m_OverheadScreenCollider = GetComponent<MeshCollider>();
        m_OverheadScreen.material.mainTexture = m_Subscriber.ImageTexture;

        m_FrontScreen.gameObject.SetActive(false);
        m_FrontScreen.material.mainTexture = m_Subscriber.ImageTexture;
        m_OverheadMapTransform.gameObject.SetActive(false);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (m_IsCameraTop)
        {
            // Toggle the front screen on and show the map
            m_FrontScreen.gameObject.SetActive(true);
            m_OverheadMapTransform.gameObject.SetActive(true);

            // Hide the overhead screen
            m_OverheadScreen.enabled = false;
            m_OverheadScreenCollider.enabled = false;
        }
        else
        {
            // Hide the map and the front screen
            m_FrontScreen.gameObject.SetActive(false);
            m_OverheadMapTransform.gameObject.SetActive(false);

            // Show the overhead screen
            m_OverheadScreen.enabled = true;
            m_OverheadScreenCollider.enabled = true;
        }
        m_IsCameraTop = !m_IsCameraTop;
    }
}
