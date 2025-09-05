using UnityEngine;
using Unity.Robotics.ROSTCPConnector;  // ROS-TCP Connector namespace
using RosMessageTypes.Std;            // For std_msgs/String

[RequireComponent(typeof(LineRenderer))]
public class MotionConstraintGuide : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "/haptic/map";

    [Header("Scaling")]
    [Tooltip("Scale factor to convert ROS units into Unity units")]
    public float scale = 1.0f;

    private LineRenderer lineRenderer;

    void Start()
    {
        // Get LineRenderer attached to this object
        lineRenderer = GetComponent<LineRenderer>();

        // Make sure the line renderer uses local space
        lineRenderer.useWorldSpace = false;

        // Subscribe to ROS topic
        ROSConnection.instance.Subscribe<StringMsg>(topicName, MapCallback);
    }

    void MapCallback(StringMsg msg)
    {
        // Example input: "x,y,z:x,y,z:x,y,z"
        string[] parts = msg.data.Split(':');
        if (parts.Length < 2)
            return;

        // Parse reference point (the first triplet)
        Vector3 reference = ParsePoint(parts[0]);

        // Parse the rest of the points relative to reference
        Vector3[] linePoints = new Vector3[parts.Length - 1];
        for (int i = 1; i < parts.Length; i++)
        {
            Vector3 point = ParsePoint(parts[i]);
            Vector3 relative = (point - reference) * scale;

            // Since useWorldSpace=false, these are in local space
            linePoints[i - 1] = relative;
        }

        // Update LineRenderer
        lineRenderer.positionCount = linePoints.Length;
        lineRenderer.SetPositions(linePoints);
    }

    private Vector3 ParsePoint(string s)
    {
        string[] coords = s.Split(',');
        if (coords.Length != 3)
            return Vector3.zero;

        float x = float.Parse(coords[0]);
        float y = float.Parse(coords[1]);
        float z = float.Parse(coords[2]);

        return new Vector3(x, y, z);
    }
}
