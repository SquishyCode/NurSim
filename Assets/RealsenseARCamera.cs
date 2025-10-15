using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor; // for CameraInfoMsg

[RequireComponent(typeof(Camera))]
public class RealSenseARCamera : MonoBehaviour
{
    [Header("ROS Settings")]
    public string cameraInfoTopic = "/camera/color/camera_info";

    [Header("UI Settings")]
    public RawImage overlayImage; // assign in inspector
    public RenderTexture cameraRenderTexture; // optional

    [Header("Virtual Marker Settings")]
    public string virtualMarkerTag = "VirtualMarker";
    public string virtualMarkerLayerName = "VirtualMarkersLayer";

    private Camera unityCamera;
    private ROSConnection ros;
    private int markerLayer;

    private int imageWidth = 640;  // default, will be updated from CameraInfo
    private int imageHeight = 480;

    void Start()
    {
        unityCamera = GetComponent<Camera>();

        // Make sure camera only clears depth (so background feed stays visible)
        unityCamera.clearFlags = CameraClearFlags.Depth;

        // Handle layer setup
        markerLayer = LayerMask.NameToLayer(virtualMarkerLayerName);
        if (markerLayer == -1)
        {
            Debug.LogError($"Layer '{virtualMarkerLayerName}' does not exist. Please add it in Unity (Edit > Project Settings > Tags and Layers).");
        }
        else
        {
            unityCamera.cullingMask = 1 << markerLayer;
        }

        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<CameraInfoMsg>(cameraInfoTopic, CameraInfoCallback);

        // Assign existing markers to the right layer
        AssignMarkersToLayer();

        // Ensure we have a render target immediately
        SetupRenderTexture(imageWidth, imageHeight);
    }

    void AssignMarkersToLayer()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag(virtualMarkerTag);
        foreach (GameObject marker in markers)
        {
            marker.layer = markerLayer;
        }
    }

    void CameraInfoCallback(CameraInfoMsg msg)
    {
        imageWidth = (int)msg.width;
        imageHeight = (int)msg.height;
        double fx = msg.k[0]; // focal length x
        double fy = msg.k[4]; // focal length y
        double cx = msg.k[2]; // principal point x
        double cy = msg.k[5]; // principal point y

        // Update projection
        Matrix4x4 proj = BuildProjectionMatrix(fx, fy, cx, cy, imageWidth, imageHeight,
                                               unityCamera.nearClipPlane, unityCamera.farClipPlane);
        unityCamera.projectionMatrix = proj;

        // Match aspect ratio
        unityCamera.aspect = (float)imageWidth / (float)imageHeight;

        // Ensure RenderTexture matches size
        SetupRenderTexture(imageWidth, imageHeight);
    }

    void SetupRenderTexture(int width, int height)
    {
        if (cameraRenderTexture == null ||
            cameraRenderTexture.width != width ||
            cameraRenderTexture.height != height)
        {
            if (cameraRenderTexture != null)
                cameraRenderTexture.Release();

            cameraRenderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            cameraRenderTexture.Create();
        }

        unityCamera.targetTexture = cameraRenderTexture;

        if (overlayImage != null)
            overlayImage.texture = cameraRenderTexture;
    }

    Matrix4x4 BuildProjectionMatrix(double fx, double fy, double cx, double cy,
                                    int width, int height, float near, float far)
    {
        float x0 = (float)(-cx) / (float)width;
        float x1 = (float)(width - cx) / (float)width;
        float y0 = (float)(cy - height) / (float)height;
        float y1 = (float)(cy) / (float)height;

        float left = near * x0 * (float)width / (float)fx;
        float right = near * x1 * (float)width / (float)fx;
        float bottom = near * y0 * (float)height / (float)fy;
        float top = near * y1 * (float)height / (float)fy;

        return PerspectiveOffCenter(left, right, bottom, top, near, far);
    }

    Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        float x = (2.0f * near) / (right - left);
        float y = (2.0f * near) / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0f * far * near) / (far - near);
        float e = -1.0f;

        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x; m[0, 1] = 0; m[0, 2] = a; m[0, 3] = 0;
        m[1, 0] = 0; m[1, 1] = y; m[1, 2] = b; m[1, 3] = 0;
        m[2, 0] = 0; m[2, 1] = 0; m[2, 2] = c; m[2, 3] = d;
        m[3, 0] = 0; m[3, 1] = 0; m[3, 2] = e; m[3, 3] = 0;
        return m;
    }
}
