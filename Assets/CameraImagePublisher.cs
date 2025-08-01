using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Sensor;
using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Std;
using Unity.Robotics.Core;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraCompressedPublisher : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "/camera/image_raw/compressed";
    public string frameId = "camera_link";
    public float publishRateHz = 10f;

    [Header("Image Settings")]
    public int imageWidth = 640;
    public int imageHeight = 480;
    [Range(1, 100)] public int jpegQuality = 75;

    private ROSConnection ros;
    private Camera cam;
    private RenderTexture renderTexture;
    private Texture2D texture2D;

    private float publishInterval;
    private float timeSinceLastPublish;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<CompressedImageMsg>(topicName);

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
            StartCoroutine(PublishCompressedImage());
        }
    }

    private IEnumerator PublishCompressedImage()
    {
        yield return new WaitForEndOfFrame();

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        cam.Render();

        texture2D.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        texture2D.Apply();
        RenderTexture.active = currentRT;

        byte[] jpgBytes = texture2D.EncodeToJPG(jpegQuality);

        HeaderMsg header = new HeaderMsg
        {
            stamp = new TimeMsg(),
            frame_id = frameId
        };

        CompressedImageMsg compressedImageMsg = new CompressedImageMsg
        {
            header = header,
            format = "jpeg",
            data = jpgBytes
        };

        ros.Publish(topicName, compressedImageMsg);
    }
}

