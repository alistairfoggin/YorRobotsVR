using UnityEngine;

public class RotateOtherTransform : MonoBehaviour
{
    [SerializeField]
    Transform m_OtherTransform;
    [SerializeField]
    float m_MinimumAngle = -180;
    [SerializeField]
    float m_MaximumAngle = 180;

    public void SetXRotation(float value)
    {
        float angle = Mathf.LerpUnclamped(m_MinimumAngle, m_MaximumAngle, value);
        Quaternion rotation = m_OtherTransform.rotation;
        rotation = Quaternion.Euler(angle, rotation.eulerAngles.y, rotation.eulerAngles.z);
        m_OtherTransform.rotation = rotation;
    }

    public void SetYRotation(float value)
    {
        float angle = Mathf.LerpUnclamped(m_MinimumAngle, m_MaximumAngle, value);
        Quaternion rotation = m_OtherTransform.rotation;
        rotation = Quaternion.Euler(rotation.eulerAngles.x, angle, rotation.eulerAngles.z);
        m_OtherTransform.rotation = rotation;
    }

    public void SetZRotation(float value)
    {
        float angle = Mathf.LerpUnclamped(m_MinimumAngle, m_MaximumAngle, value);
        Quaternion rotation = m_OtherTransform.rotation;
        rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, angle);
        m_OtherTransform.rotation = rotation;
    }
}
