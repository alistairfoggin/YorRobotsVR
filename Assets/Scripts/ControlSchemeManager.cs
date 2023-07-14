using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSchemeManager : MonoBehaviour
{
    public GameObject rayControllerGameObject;
    public GameObject teleportationGameObject;
    public GameObject directControllerGameObject;

    public InputActionReference teleportActivationReference;
    public InputActionReference directActivationReference;

    // Start is called before the first frame update
    void Start()
    {
        if (teleportationGameObject != null)
        {
            teleportActivationReference.action.performed += EnableTeleportMode;
            teleportActivationReference.action.canceled += DisableTeleportMode;
            teleportationGameObject.SetActive(false);
        }
        rayControllerGameObject.SetActive(true);
        directControllerGameObject.SetActive(false);

    }

    public void EnterDirectInteraction()
    {
        rayControllerGameObject.SetActive(false);
        teleportationGameObject.SetActive(false);
        directControllerGameObject.SetActive(true);
    }

    public void ExitDirectInteraction()
    {
        if (!directControllerGameObject.activeInHierarchy)
        {
            return;
        }
        rayControllerGameObject.SetActive(true);
        teleportationGameObject.SetActive(false);
        directControllerGameObject.SetActive(false);
    }

    private void EnableTeleportMode(InputAction.CallbackContext obj)
    {
        Invoke("EnableTeleport", .1f);
    }

    void EnableTeleport()
    {
        rayControllerGameObject.SetActive(false);
        teleportationGameObject.SetActive(true);
        directControllerGameObject.SetActive(false);
    }

    private void DisableTeleportMode(InputAction.CallbackContext obj)
    {
        if (!teleportationGameObject.activeInHierarchy)
        {
            return;
        }
        Invoke("DisableTeleport", .1f);
    }

    void DisableTeleport()
    {
        rayControllerGameObject.SetActive(true);
        teleportationGameObject.SetActive(false);
        directControllerGameObject.SetActive(false);
    }
}
