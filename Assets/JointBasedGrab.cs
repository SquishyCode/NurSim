using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class JointBasedGrab : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private FixedJoint joint;
    private GameObject jointAnchor;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = false;

        // Disable XR Toolkit's built-in movement
        grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRBaseInteractor interactor)
        {
            // Create a joint anchor at the grab point
            jointAnchor = new GameObject("JointAnchor");
            jointAnchor.transform.position = interactor.transform.position;
            jointAnchor.transform.rotation = interactor.transform.rotation;
            jointAnchor.transform.SetParent(interactor.transform);

            // Add a joint to the object and connect it to the anchor
            joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = jointAnchor.AddComponent<Rigidbody>();
            joint.connectedBody.isKinematic = true;
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
        }
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }

        if (jointAnchor != null)
        {
            Destroy(jointAnchor);
            jointAnchor = null;
        }
    }
}
