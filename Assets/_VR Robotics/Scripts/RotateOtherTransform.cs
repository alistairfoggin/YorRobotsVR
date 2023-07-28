using UnityEngine;

public class RotateOtherTransform : MonoBehaviour
{
    [SerializeField]
    Transform otherTransform;

    public void SetXRotation(float value)
    {
        float angle = Mathf.LerpUnclamped(-90, 0, value);
        Quaternion rotation = otherTransform.rotation;
        rotation = Quaternion.Euler(angle, rotation.eulerAngles.y, rotation.eulerAngles.z);
        otherTransform.rotation = rotation;
    }

    public void SetYRotation(float value)
    {
        float angle = Mathf.LerpUnclamped(-180, 180, value);
        Quaternion rotation = otherTransform.rotation;
        rotation = Quaternion.Euler(rotation.eulerAngles.x, angle, rotation.eulerAngles.z);
        otherTransform.rotation = rotation;
    }

    public void SetZRotation(float value)
    {
        float angle = Mathf.LerpUnclamped(-180, 180, value);
        Quaternion rotation = otherTransform.rotation;
        rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, angle);
        otherTransform.rotation = rotation;
    }
}
