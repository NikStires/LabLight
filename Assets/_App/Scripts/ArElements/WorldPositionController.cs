using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applies smoothed trackedObject position and orientation to the view. 
/// When TrackedObject is not available anymore it uses the default position/orientation
/// 
/// Uses SlotProvider to acquire a free position to place the object when it is not detected yet
/// </summary>

public class WorldPositionController : ArObjectViewController
{
    protected Vector3 currentVelocity;
    protected Quaternion currentQuaternionVelocity;
    protected float smoothTime = .1f;

    protected Transform _defaultSlot;
    protected Vector3 _defaultPosition = Vector3.zero;

    public bool selectedForLocking = false;
    public bool positionLocked = false;
    public bool lockedToTablePlane = true;
    public bool positionValid = true;
    protected Vector3 positionOnLock;

    public bool hasBeenLocked = false;


    public override void Initialize(ArObject arObject, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arObject, trackedObjects);

        //implement grabbable objects here


        // Jump to initial position
        if(GetCondition() != null && GetCondition().conditionType == ConditionType.Target)
        {
            if (TrackedObjects != null)
            {
                if (TrackedObjects.Count == 1)
                {
                    if (!positionLocked)
                    {
                        transform.localPosition = TrackedObjects[0].position;
                        //transform.localRotation = this.TrackedObjects[0].rotation;
                    }
                }
                else
                {
                    // TODO Handle multiple 
                }
            }
            else
            {

                //transform.position = _defaultPosition;
                if(SessionManager.instance.CharucoTransform != null)
                {
                    transform.position = SessionManager.instance.CharucoTransform.position;
                // }else
                // {
                //     transform.position = ((ModelArObject)this.arObject).position;
                // }
                }
            }
        }
        else
        {
            //transform.position = ((ModelArDefinition)this.arDefinition).position; removed for debugging purposes
            if(SessionManager.instance.CharucoTransform != null)
            {
                transform.position = SessionManager.instance.CharucoTransform.position;
            }else
            {
                transform.position = Vector3.zero;
            }
        }
    }

    public void UnlockPosition()
    {
        // positionLocked = false;
        // lockedToTablePlane = false;
        //hasBeenLocked = false; //should reset model to original slot position
    }

    public void LockPosition()
    {
        // hasBeenLocked = true;
        // positionLocked = true;
        // if(GetCondition() != null && GetCondition().conditionType == ConditionType.Anchor)
        // {
        //     if(this.GetArDefinitionType() == ArDefinitionType.Model)
        //     {
        //         lockedToTablePlane = true;
        //         transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        //     }
        //     else
        //     {
        //         lockedToTablePlane = false;
        //     }
        // }
        // positionOnLock = transform.localPosition;
        //transform.localRotation = Quaternion.identity;
    }

    // public void SetPosition(Vector3 pos)
    // {
    //     positionLocked = true;
    //     lockedToTablePlane = false;
    //     positionOnLock = pos;
    // }

    //public virtual void Update()
    //{
        // Vector3 target; used for object detection, depricated
        // // Position in the middle of the object (z is negative)
        // if(selectedForLocking || this.arObject.IsGeneric())
        // {
        //     if (TrackedObjects != null && TrackedObjects.Count == 1)
        //     {
        //         //target = TrackedObjects[0].position;
        //         target = (lockedToTablePlane ? NNPostProcessing.roundToCM(new Vector3(TrackedObjects[0].position.x, 0, TrackedObjects[0].position.z)) : NNPostProcessing.roundToCM(TrackedObjects[0].position));
        //         Quaternion rotationTarget = TrackedObjects[0].rotation;
        //         transform.localRotation = QuaternionUtil.SmoothDamp(transform.localRotation, rotationTarget, ref currentQuaternionVelocity, smoothTime); 
        //         transform.localPosition = Vector3.SmoothDamp(transform.localPosition, target, ref currentVelocity, smoothTime);
        //     }
        // }else if(!hasBeenLocked && _defaultSlot != null)
        // {
        //     target = _defaultSlot.position;
        //     transform.localPosition = Vector3.SmoothDamp(transform.localPosition, target, ref currentVelocity, smoothTime);
        // }
    //}

    // public ArObjectType GetArDefinitionType()
    // {
    //     return this.arDefinition.arDefinitionType;
    // }

    // public Condition GetCondition()
    // {
    //     return this.arDefinition.condition;
    // }
}
