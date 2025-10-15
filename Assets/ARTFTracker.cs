using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Tf2;
using RosMessageTypes.Geometry;
using System.Collections.Generic;

public class PivotingTFTracker : MonoBehaviour
{
    [Header("TF Settings")]
    [Tooltip("The ROS frame of the object to track.")]
    public string targetFrame;        // e.g., "ball"
    [Tooltip("The ROS frame of the camera.")]
    public string cameraFrame;        // e.g., "camera_color_optical_frame"

    [Header("Camera Reference")]
    [Tooltip("Unity virtual camera representing the real camera.")]
    public Transform virtualCamera;

    [Header("ROS Settings")]
    [Tooltip("ROS topic for TF messages.")]
    public string tfTopic = "/tf";

    private ROSConnection ros;
    private Dictionary<string, TransformStampedMsg> latestTransforms = new Dictionary<string, TransformStampedMsg>();

    public bool tfReceived = false;

    void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual camera not assigned!");
            enabled = false;
            return;
        }

        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<TFMessageMsg>(tfTopic, OnTFMessageReceived);
    }

    void OnTFMessageReceived(TFMessageMsg msg)
    {
        tfReceived = true;

        foreach (var t in msg.transforms)
        {
            string child = NormalizeFrame(t.child_frame_id);
            latestTransforms[child] = t;
        }
    }

    void Update()
    {
        if (!tfReceived)
            return;

        string targetKey = NormalizeFrame(targetFrame);
        string cameraKey = NormalizeFrame(cameraFrame);

        if (!latestTransforms.ContainsKey(targetKey) || !latestTransforms.ContainsKey(cameraKey))
            return;

        // Get latest transforms
        TransformStampedMsg targetTf = latestTransforms[targetKey];
        TransformStampedMsg cameraTf = latestTransforms[cameraKey];

        // Convert ROS -> Unity
        Vector3 targetPosUnity = RosVectorToUnity(targetTf.transform.translation);
        Quaternion targetRotUnity = RosQuatToUnity(targetTf.transform.rotation);

        Vector3 cameraPosUnity = RosVectorToUnity(cameraTf.transform.translation);
        Quaternion cameraRotUnity = RosQuatToUnity(cameraTf.transform.rotation);

        // Compute position relative to camera (pivot around camera)
        Vector3 relativePos = Quaternion.Inverse(cameraRotUnity) * (targetPosUnity - cameraPosUnity);
        Quaternion relativeRot = Quaternion.Inverse(cameraRotUnity) * targetRotUnity;

        // Apply pivoted transform relative to Unity camera
        transform.position = virtualCamera.position + virtualCamera.rotation * relativePos;
        transform.rotation = virtualCamera.rotation * relativeRot;
    }

    // Normalize frame name (remove leading '/')
    string NormalizeFrame(string frame)
    {
        return frame.StartsWith("/") ? frame.Substring(1) : frame;
    }

    // Convert ROS Vector3 (RealSense optical frame) to Unity coordinates
    Vector3 RosVectorToUnity(Vector3Msg v)
    {
        // ROS optical: x-right, y-down, z-forward
        // Unity: x-right, y-up, z-forward
        return new Vector3((float)v.x, -(float)v.y, (float)v.z);
    }

    // Convert ROS Quaternion to Unity
    Quaternion RosQuatToUnity(QuaternionMsg q)
    {
        // ROS optical frame to Unity: invert Y
        return new Quaternion((float)q.x, -(float)q.y, (float)q.z, (float)q.w);
    }
}
