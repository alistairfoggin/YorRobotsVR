using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSchemeManager : MonoBehaviour
{
    public GameObject rayControllerGameObject;
    public GameObject teleportationGameObject;
    public GameObject directControllerGameObject;

    public InputActionReference teleportActivationReference;

    // Start is called before the first frame update
    void Start()
    {
        teleportActivationReference.action.performed += EnableTeleportMode;
        teleportActivationReference.action.canceled += DisableTeleportMode;
        rayControllerGameObject.SetActive(true);
        teleportationGameObject.SetActive(false);
        directControllerGameObject.SetActive(false);

    }

    private void OnTriggerEnter(Collider other)
    {
        print("Trigger Entered!");
        if (teleportationGameObject.activeSelf)
        {
            return;
        }
        rayControllerGameObject.SetActive(false);
        teleportationGameObject.SetActive(false);
        directControllerGameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!directControllerGameObject.activeSelf)
        {
            return;
        }
        rayControllerGameObject.SetActive(true);
        teleportationGameObject.SetActive(false);
        directControllerGameObject.SetActive(false);
    }

    private void EnableTeleportMode(InputAction.CallbackContext obj)
    {
        if (directControllerGameObject.activeSelf)
        {
            return;
        }
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
        if (!teleportationGameObject.activeSelf)
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
