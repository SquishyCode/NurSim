using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class UnityBallReciver : MonoBehaviour
{
    [Tooltip("Unity virtual camera representing the real camera.")]
    public Transform virtualCamera;

    [Tooltip("ROS topic publishing the relative position.")]
    public string topicName = "/unity_ball_relative_position";

    private ROSConnection ros;

    void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual camera not assigned!");
            enabled = false;
            return;
        }

        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PointMsg>(topicName, OnPositionReceived);
    }

    private void OnPositionReceived(PointMsg msg)
    {
        Vector3 relativePos = new Vector3((float)msg.x, (float)msg.y, (float)msg.z);

        // Apply relative to virtual camera
        transform.position = virtualCamera.position + virtualCamera.rotation * relativePos;
    }
}
