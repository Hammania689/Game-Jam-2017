using UnityEngine;
using System.Collections;

public class IKHandler : MonoBehaviour
{
    #region Global Variable Declaration

    [Header("IK Weight Variables")]
    public float lookWeight = 1;
    public float bodyWeight = 0.8f;
    public float headWeight = 1;
    public float clampWeight = 1;

    [Header("General IK Positioning")]
    public Transform weaponHolder;
    public Transform rightShoulder;
    public Transform overrideLookTarget;
    public Transform aimHelper;

    [Header("Hands IK Positions and Weight")]
    public Transform rightHandIkTarget;
    public float rightHandIKWeight;
    public Transform leftHandIKTarget;
    public float leftHandIKWeight;

    Animator anim;
    StateManager states;

    public bool notFacing = false;

    float targetWeight;

    #endregion

    void Start()
    {
        aimHelper = new GameObject("Aim Helper").transform;
        anim = GetComponent<Animator>();
        states = GetComponent<StateManager>();
    }

    void Update()
    {
        if (states.shoot)
        {
            states.notFacing = notFacing;
        }
        else
        {
            states.notFacing = false;
        }
    }

    void FixedUpdate()
    {
        if (rightShoulder == null)
        {
            rightShoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder);
        }
        else
        {
            weaponHolder.position = rightShoulder.position;
        }

        if (states.aiming && !states.reloading)
        {
            Vector3 directionTowardsTarget = aimHelper.position - transform.position;
            float angle = Vector3.Angle(transform.forward, directionTowardsTarget);

            if (angle < 90)
            {
                targetWeight = 1;
                notFacing = false;
            }
            else
            {
                targetWeight = 0;
                notFacing = true;
            }
        }
        else if (!states.aiming && !states.reloading && states.shoot)
        {
            Vector3 directionTowardsTarget = aimHelper.position - transform.position;
            float angle = Vector3.Angle(transform.forward, directionTowardsTarget);

            if (angle < 90)
            {
                targetWeight = 1;
                notFacing = false;
            }
            else
            {
                targetWeight = 0;
                notFacing = true;
            }
        }
        else
        {
            targetWeight = 0;
        }

        float multiplier = (states.aiming) ? 5 : 30;

        lookWeight = Mathf.Lerp(lookWeight, targetWeight, Time.deltaTime * multiplier);

        rightHandIKWeight = lookWeight;

        leftHandIKWeight = 1 - anim.GetFloat("LeftHandIKWeightOverride");

        HandleShoulderRotation();

    }

    void HandleShoulderRotation()
    {
        aimHelper.position = Vector3.Lerp(aimHelper.position, states.lookPosition, Time.deltaTime * 5);
        weaponHolder.LookAt(aimHelper.position);
        rightHandIkTarget.parent.transform.LookAt(aimHelper.position);
    }

    void OnAnimatorIK()
    {
        anim.SetLookAtWeight(lookWeight, bodyWeight, headWeight, headWeight, clampWeight);

        Vector3 filterDirection = states.lookPosition;
        //filterDirection.y = offsetY; //if needed
        anim.SetLookAtPosition((overrideLookTarget != null) ? overrideLookTarget.position : filterDirection);

        if (leftHandIKTarget != null)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
        }

        if (rightHandIkTarget != null)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
            anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandIkTarget.position);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
            anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandIkTarget.rotation);
        }
    }
}

