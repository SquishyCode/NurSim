using UnityEngine;
using UnityEngine.Animations.Rigging;

public class VRRigAutoBinder : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public RigBuilder rigBuilder;
    [Header("Constraints")]
    public MultiAimConstraint headAimConstraint;
    public TwoBoneIKConstraint leftArmIK;
    public TwoBoneIKConstraint rightArmIK;

    [Header("VR Transforms")]
    public Transform vrHead;
    public Transform vrLeftHand;
    public Transform vrRightHand;

    [Header("IK Targets")]
    public Transform headTarget;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    void Start()
    {
        if (animator == null)
        {
            Debug.LogError("Animator not assigned.");
            return;
        }

        SetupHeadAim();
        SetupArmIK(leftArmIK, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand, leftHandTarget);
            SetupArmIK(rightArmIK, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand, rightHandTarget);
            rigBuilder.Build();
    }

    void SetupHeadAim()
    {
        if (headAimConstraint == null || headTarget == null)
            return;

        var headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        if (headBone == null)
        {
            Debug.LogError("Head bone not found.");
            return;
        }

        headAimConstraint.data.constrainedObject = headBone;
        headAimConstraint.data.sourceObjects.Clear();
        headAimConstraint.data.sourceObjects.Add(new WeightedTransform(headTarget, 1f));
        headAimConstraint.weight = 1f;
    }

    void SetupArmIK(TwoBoneIKConstraint constraint, HumanBodyBones upperArmBone, HumanBodyBones forearmBone, HumanBodyBones handBone, Transform target)
    {
        if (constraint == null || target == null)
            return;

        var upperArm = animator.GetBoneTransform(upperArmBone);
        var forearm = animator.GetBoneTransform(forearmBone);
        var hand = animator.GetBoneTransform(handBone);

        if (!upperArm || !forearm || !hand)
        {
            Debug.LogError("One or more bones missing for IK setup.");
            return;
        }

        constraint.data.root = upperArm;
        constraint.data.mid = forearm;
        constraint.data.tip = hand;
        constraint.data.target = target;

        // Optional: create and assign a pole target to stabilize elbow
        GameObject pole = new GameObject(target.name + "_Pole");
        pole.transform.position = upperArm.position + Vector3.right * 0.2f; // simple offset
        constraint.data.hint = pole.transform;

        constraint.weight = 1f;
    }

    void LateUpdate()
    {
        // Move IK targets with VR
        if (headTarget && vrHead)
        {
            headTarget.position = vrHead.position;
            headTarget.rotation = vrHead.rotation;
        }

        if (leftHandTarget && vrLeftHand)
        {
            leftHandTarget.position = vrLeftHand.position;
            leftHandTarget.rotation = vrLeftHand.rotation;
        }

        if (rightHandTarget && vrRightHand)
        {
            rightHandTarget.position = vrRightHand.position;
            rightHandTarget.rotation = vrRightHand.rotation;
        }
    }
}
