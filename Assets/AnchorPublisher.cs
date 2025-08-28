using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Tf2;
using RosMessageTypes.Geometry;

public class AnchorPublisher : MonoBehaviour
{
    [Header("Anchor Frame Settings")]
    public string anchorFrameName = "anchor";

    [Serializable]
    public class NamedObject
    {
        public string frameName;
        public GameObject targetObject;
    }

    [Header("Frames to Publish (relative to anchor)")]
    public List<NamedObject> trackedObjects = new List<NamedObject>();

    ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TFMessageMsg>("/tf");
    }

double GetUnixTime()
{
    DateTime utcNow = DateTime.UtcNow;
    DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    return (utcNow - unixEpoch).TotalSeconds;
}


    void Update()
    {
        double time = GetUnixTime(); 
        List<TransformStampedMsg> tfList = new List<TransformStampedMsg>();

        foreach (var namedObj in trackedObjects)
        {
            if (namedObj.targetObject == null) continue;

            Vector3 localPos = transform.InverseTransformPoint(namedObj.targetObject.transform.position);
            Quaternion localRot = Quaternion.Inverse(transform.rotation) * namedObj.targetObject.transform.rotation;

            tfList.Add(CreateTransformStamped(
                anchorFrameName,
                namedObj.frameName,
                localPos,
                localRot,
                time
            ));
        }

        TFMessageMsg tfMessage = new TFMessageMsg
        {
            transforms = tfList.ToArray()
        };

        ros.Publish("/tf", tfMessage);
    }

    TransformStampedMsg CreateTransformStamped(string parentFrame, string childFrame, Vector3 unityPosition, Quaternion unityRotation, double time)
    {
        // === POSITION CONVERSION ===
        // Matches your provided Python mapping exactly
        float rosX = unityPosition.z;
        float rosY = -unityPosition.x;
        float rosZ = unityPosition.y;

        // === ROTATION CONVERSION ===
        // Python: newX = -oldZ, newY = oldX, newZ = -oldY, newW = oldW
        float rosQx = -unityRotation.z;
        float rosQy = unityRotation.x;
        float rosQz = -unityRotation.y;
        float rosQw = unityRotation.w;

        return new TransformStampedMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg
            {
                frame_id = parentFrame,
                stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg
                {
                    sec = (uint)time,
                    nanosec = (uint)((time - Math.Floor(time)) * 1e9)
                }
            },
            child_frame_id = childFrame,
            transform = new TransformMsg
            {
                translation = new Vector3Msg(rosX, rosY, rosZ),
                rotation = new QuaternionMsg(rosQx, rosQy, rosQz, rosQw)
            }
        };
    }
}
