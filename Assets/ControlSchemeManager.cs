using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ControlSchemeManager : MonoBehaviour
{
    public GameObject baseControllerGameObject;
    public GameObject teleportationGameObject;

    public InputActionReference teleportActivationReference;

    // Start is called before the first frame update
    void Start()
    {
        teleportActivationReference.action.performed += EnableTeleportMode;
        teleportActivationReference.action.canceled += DisableTeleportMode;
        baseControllerGameObject.SetActive(true);
        teleportationGameObject.SetActive(false);
    }

    private void EnableTeleportMode(InputAction.CallbackContext obj)
    {
        Invoke("EnableTeleport", .1f);
    }

    void EnableTeleport()
    {
            baseControllerGameObject.SetActive(false);
            teleportationGameObject.SetActive(true);
    }

    private void DisableTeleportMode(InputAction.CallbackContext obj)
    {
        Invoke("DisableTeleport", .1f);
    }

    void DisableTeleport()
    {
        baseControllerGameObject.SetActive(true);
        teleportationGameObject.SetActive(false);
    }
}
