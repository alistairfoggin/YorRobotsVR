using UnityEngine;

public class RotateOtherTransform : MonoBehaviour
{
    [SerializeField]
    Transform otherTransform;

    public void SetYRotation(float value)
    {
        float angle = Mathf.LerpUnclamped(-180, 180, value);
        Quaternion rotation = otherTransform.rotation;
        rotation = Quaternion.Euler(rotation.eulerAngles.x, angle, rotation.eulerAngles.z);
        otherTransform.rotation = rotation;
    }
}
