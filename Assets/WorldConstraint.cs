using UnityEngine;

[ExecuteAlways]
public class WorldConstraint : MonoBehaviour
{
    [Header("Position Settings")]
    public Transform parent;
    
    [Tooltip("Enable world position following on these axes.")]
    public bool followX = true;
    public bool followY = true;
    public bool followZ = true;

    [Tooltip("Offsets applied in world space.")]
    public Vector3 positionOffset = Vector3.zero;

    [Header("Rotation Settings")]
    [Tooltip("Lock rotation axes in world space.")]
    public bool lockRotX = false;
    public bool lockRotY = false;
    public bool lockRotZ = false;

    [Tooltip("Override world rotation values (only used if lock is checked).")]
    public Vector3 rotationOverride = Vector3.zero;

    void LateUpdate()
    {
        if (parent == null) return;

        // ----- Position -----
        Vector3 newPos = transform.position;

        if (followX) newPos.x = parent.position.x + positionOffset.x;
        if (followY) newPos.y = parent.position.y + positionOffset.y;
        if (followZ) newPos.z = parent.position.z + positionOffset.z;

        transform.position = newPos;

        // ----- Rotation -----
        Vector3 newEuler = transform.eulerAngles;

        if (lockRotX) newEuler.x = rotationOverride.x;
        if (lockRotY) newEuler.y = rotationOverride.y;
        if (lockRotZ) newEuler.z = rotationOverride.z;

        transform.rotation = Quaternion.Euler(newEuler);
    }
}
