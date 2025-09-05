using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using UnityEngine.XR.ARFoundation;  // Required for ARCameraManager

public class PassthroughToggle : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "/passthrough/toggle";
    public string triggerKey = "ENABLE";  // The string to trigger passthrough mode

    [Header("Objects to Disable in Passthrough")]
    public GameObject[] objectsToDisable;

    [Header("Camera Settings")]
    public Camera targetCamera;

    private CameraClearFlags originalClearFlags;
    private Color originalBackgroundColor;
    private bool isPassthrough = false;

    // Reference to ARCameraManager
    private ARCameraManager arCameraManager;

    void Start()
    {
        if (targetCamera == null)
        {
            Debug.LogError("PassthroughToggle: No targetCamera assigned!");
            return;
        }

        // Store original camera background settings
        originalClearFlags = targetCamera.clearFlags;
        originalBackgroundColor = targetCamera.backgroundColor;

        // Get ARCameraManager if it exists
        arCameraManager = targetCamera.GetComponent<ARCameraManager>();
        if (arCameraManager == null)
        {
            Debug.LogWarning("PassthroughToggle: No ARCameraManager found on targetCamera.");
        }

        // Start with passthrough disabled (skybox visible)
        ExitPassthrough();

        // Subscribe to ROS topic
        ROSConnection.instance.Subscribe<StringMsg>(topicName, ToggleCallback);
    }

    void ToggleCallback(StringMsg msg)
    {
        if (msg.data == triggerKey && !isPassthrough)
        {
            EnterPassthrough();
        }
        else if (msg.data != triggerKey && isPassthrough)
        {
            ExitPassthrough();
        }
    }

    private void EnterPassthrough()
    {
        isPassthrough = true;

        // Disable objects
        foreach (var obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Set camera to transparent black
        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = new Color(0, 0, 0, 0);

        // Re-enable ARCameraManager
        if (arCameraManager != null)
            arCameraManager.enabled = true;
    }

    private void ExitPassthrough()
    {
        isPassthrough = false;

        // Re-enable objects
        foreach (var obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // Restore camera background
        targetCamera.clearFlags = originalClearFlags;
        targetCamera.backgroundColor = originalBackgroundColor;

        // Disable ARCameraManager
        if (arCameraManager != null)
            arCameraManager.enabled = false;
    }
}
