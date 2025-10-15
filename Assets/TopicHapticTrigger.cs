using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std; // For std_msgs/String
using UnityEngine.XR;
using System.Collections;

public class TopicHapticTrigger : MonoBehaviour
{

    public enum TopicType
    {
        STRING,
        BOOL
    }
    [Header("ROS Settings")]
    [Tooltip("Name of the ROS topic to subscribe to.")]
    
    [SerializeField] public TopicType topicType = TopicType.STRING;
    public string rosTopicName = "/my_string_topic";

    public string[] triggers;

    [Header("Haptics Settings")]
    public XRNode handNode = XRNode.RightHand; // LeftHand or RightHand
    [Range(0f, 1f)]
    public float amplitude = 0.5f; // Strength of the vibration
    [Range(0f, 1f)]
    public float frequency = 0.5f; // Frequency of vibration
    public float duration = 0.2f; // Seconds

    // Private state
    ROSConnection ros;
    string lastMessage = "";

    void Start()
    {
        ros = ROSConnection.instance;
        if (ros == null)
        {
            Debug.LogError("ROSConnection.instance not found! Please add ROSConnection to the scene.");
            return;
        }

        switch (topicType)
        {
            case TopicType.STRING: ros.Subscribe<StringMsg>(rosTopicName, OnRosMessageString); break;
            case TopicType.BOOL: ros.Subscribe<BoolMsg>(rosTopicName, OnRosMessageBool); break;
        }
    }

    void OnRosMessageString(StringMsg msg)
    {
        string newMessage = msg.data;

        // Only trigger if the message content changed
        if (newMessage != lastMessage)
        {
            lastMessage = newMessage;
            for (int i = 0; i < triggers.Length; i++)
            {
                if (newMessage == triggers[i])
                {
                    TriggerHaptics();
                }
            }
            
        }
    }

    void OnRosMessageBool(BoolMsg msg)
    {
        string newMessage = msg.data?"true":"false";

        // Only trigger if the message content changed
        if (newMessage != lastMessage)
        {
            lastMessage = newMessage;
            for (int i = 0; i < triggers.Length; i++)
            {
                if (newMessage == triggers[i])
                {
                    TriggerHaptics();
                }
            }
            
        }
    }

    void TriggerHaptics()
    {
        // Unity OpenXR haptics via InputDevices API
        InputDevice device = InputDevices.GetDeviceAtXRNode(handNode);

        if (device.isValid && device.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse)
        {
            uint channel = 0; // Usually 0 for most devices
            device.SendHapticImpulse(channel, amplitude, duration);
            Debug.Log($"Haptic pulse triggered on {handNode}: amplitude={amplitude}, duration={duration}");
        }
        else
        {
            Debug.LogWarning($"Haptics not supported or device not found for {handNode}");
        }
    }
}
