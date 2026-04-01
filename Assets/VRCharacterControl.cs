using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class VRCharacterControl : MonoBehaviour
{
      [Header("Input Actions")]
    public InputActionProperty moveAction; // Right joystick (Vector2)
    public InputActionProperty headsetRotationAction; // Headset rotation (Quaternion)

    [Header("Movement Settings")]
    public float moveSpeed = 1.5f;

    [Header("Bobbing Settings")]
    public float bobbingIntensity = 0.05f;
    public float bobbingSpeed = 6.0f;

    private float bobbingTimer = 0f;
    private Vector3 baseLocalPos;

    //Added Rigidbody for Collisions
    private Rigidbody rb;


    void OnEnable()
    {
        baseLocalPos = transform.localPosition;
        moveAction.action.Enable();
        headsetRotationAction.action?.Enable();

        if(rb != null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        headsetRotationAction.action.Disable();
    }

    void Update()
    {
        if (moveAction == null || headsetRotationAction == null)
            return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        if (input.sqrMagnitude > 0.01f)
        {
            // Get headset rotation (assumed to be Quaternion)
            Quaternion headsetRotation = headsetRotationAction.action.ReadValue<Quaternion>();

            // Isolate YAW only (ignore pitch/roll)
            Vector3 headsetForward = headsetRotation * Vector3.forward;
            headsetForward.y = 0f;
            headsetForward.Normalize();

            Vector3 headsetRight = Quaternion.Euler(0, 90, 0) * headsetForward;

            // Build movement vector aligned with headset direction
            Vector3 moveDirection = headsetForward * input.y + headsetRight * input.x;
            Vector3 movement = moveDirection.normalized * moveSpeed * Time.deltaTime;


            if (rb != null)
            {
                //Added code to work when rigidbody is there
                rb.MovePosition(rb.position + movement);
            }
            else
            {
                //Original Code occurs if rigidbody is null
                transform.position += movement;
            }

            ApplyBobbing(input);
        }
        else
        {
            ResetBobbing();
        }
    }

    private void ApplyBobbing(Vector2 input)
    {
        if (bobbingIntensity <= 0f) return;

        bobbingTimer += Time.deltaTime * bobbingSpeed;
        float offset = Mathf.Sin(bobbingTimer) * bobbingIntensity;

        transform.localPosition = new Vector3(
            transform.localPosition.x,
            baseLocalPos.y + offset,
            transform.localPosition.z
        );
    }

    private void ResetBobbing()
    {
        bobbingTimer = 0f;
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            baseLocalPos.y,
            transform.localPosition.z
        );
    }
}
