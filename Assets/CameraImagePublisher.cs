using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Sensor;
using System.Collections;
using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Std;
using Unity.Robotics.Core;

[RequireComponent(typeof(Camera))]
public class CameraImagePublisher : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "/camera/image_raw";
    public string frameId = "camera_link";
    public float publishRateHz = 10f;

    [Header("Image Settings")]
    public int imageWidth = 640;
    public int imageHeight = 480;
    public string encoding = "rgb8";  // Other options: mono8, bgr8, etc.

    private ROSConnection ros;
    private Camera cam;
    private RenderTexture renderTexture;
    private Texture2D texture2D;

    private float publishInterval;
    private float timeSinceLastPublish;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);

        cam = GetComponent<Camera>();
        cam.targetTexture = new RenderTexture(imageWidth, imageHeight, 24);
        renderTexture = cam.targetTexture;

        texture2D = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);

        publishInterval = 1.0f / publishRateHz;
    }

    void Update()
    {
        timeSinceLastPublish += Time.deltaTime;
        if (timeSinceLastPublish >= publishInterval)
        {
            timeSinceLastPublish = 0f;
            StartCoroutine(PublishCameraImage());
        }
    }

    private IEnumerator PublishCameraImage()
    {
        yield return new WaitForEndOfFrame();

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        cam.Render();

        texture2D.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        texture2D.Apply();
        RenderTexture.active = currentRT;

        // Get raw image data
        byte[] rawData = texture2D.GetRawTextureData();

        // Flip vertically
        byte[] flippedData = new byte[rawData.Length];
        int rowSize = imageWidth * 3;

        for (int y = 0; y < imageHeight; y++)
        {
            int srcIndex = y * rowSize;
            int dstIndex = (imageHeight - 1 - y) * rowSize;
            System.Buffer.BlockCopy(rawData, srcIndex, flippedData, dstIndex, rowSize);
        }

        // Create header
        HeaderMsg header = new HeaderMsg
        {
            stamp = new TimeMsg(),
            frame_id = frameId
        };

        // Create the Image message
        ImageMsg imageMessage = new ImageMsg
        {
            header = header,
            height = (uint)imageHeight,
            width = (uint)imageWidth,
            encoding = encoding,
            is_bigendian = 0,
            step = (uint)(imageWidth * 3),  // 3 bytes per pixel for RGB
            data = flippedData
        };

        ros.Publish(topicName, imageMessage);
    }
}
