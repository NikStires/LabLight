using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

/// <summary>
/// Applies smoothed trackedObject position and orientation to the view. 
/// When TrackedObject is not available anymore it uses the default position/orientation
/// 
/// Uses SlotProvider to acquire a free position to place the object when it is not detected yet
/// </summary>

[RequireComponent(typeof(NearInteractionGrabbable))]
[RequireComponent(typeof(ObjectManipulator))]
[RequireComponent(typeof(RotationAxisConstraint))]
public class WorldPositionController : ArElementViewController
{
    protected Vector3 currentVelocity;
    protected Quaternion currentQuaternionVelocity;
    protected float smoothTime = .1f;
    protected ISlotProvider _slotProvider;

    protected Transform _defaultSlot;
    protected Vector3 _defaultPosition = Vector3.zero;
    protected Quaternion _defaultOrientation = Quaternion.identity;

    public bool selectedForLocking = false;
    public bool positionLocked = false;
    public bool lockedToTablePlane = true;
    public bool positionValid = true;
    protected Vector3 positionOnLock;

    public bool hasBeenLocked = false;

    protected NearInteractionGrabbable _nearInteractionGrabbable;
    protected ObjectManipulator _objectManipulator;

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);

        _nearInteractionGrabbable = GetComponent<NearInteractionGrabbable>();
        _nearInteractionGrabbable.enabled = false;
        _objectManipulator = GetComponent<ObjectManipulator>();
        _objectManipulator.enabled = false;

        _slotProvider = ServiceRegistry.GetService<ISlotProvider>();
        if (_slotProvider != null)
        {
            _defaultSlot = _slotProvider.GetFreeSlot();
            if (_defaultSlot != null)
            {
                _defaultPosition = SessionManager.Instance.CharucoTransform.InverseTransformPoint(_defaultSlot.position);
                //_defaultPosition = _defaultSlot.position;
            }
        }

        // Jump to initial position
        if(GetCondition() != null && GetCondition().conditionType == ConditionType.Target)
        {
            if (TrackedObjects != null)
            {
                if (TrackedObjects.Count == 1)
                {
                    if (!positionLocked)
                    {
                        transform.localPosition = this.TrackedObjects[0].position;
                        transform.localRotation = this.TrackedObjects[0].rotation;
                    }
                }
                else
                {
                    // TODO Handle multiple 
                }
            }
            else
            {
                transform.position = _defaultPosition;
                transform.localRotation = _defaultOrientation;
            }
        }
        else
        {
            transform.position = _defaultPosition;
            transform.localRotation = _defaultOrientation;
        }
    }

    private void OnDestroy()
    {
        if (_slotProvider != null && _defaultSlot != null)
        {
            _slotProvider.ReturnSlot(_defaultSlot);
        }
    }

    public void UnlockPosition()
    {
        positionLocked = false;
        lockedToTablePlane = false;
        _nearInteractionGrabbable.enabled = true;
        _objectManipulator.enabled = true;
        //hasBeenLocked = false; //should reset model to original slot position
    }

    public void LockPosition()
    {
        hasBeenLocked = true;
        positionLocked = true;
        if(GetCondition() != null && GetCondition().conditionType == ConditionType.Anchor)
        {
            if(this.GetArDefinitionType() == ArDefinitionType.Model)
            {
                lockedToTablePlane = true;
                transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
            }
            else
            {
                lockedToTablePlane = false;
            }
        }
        positionOnLock = transform.localPosition;
        transform.localRotation = Quaternion.identity;
        _nearInteractionGrabbable.enabled = false;
        _objectManipulator.enabled = false;
    }

    // public void SetPosition(Vector3 pos)
    // {
    //     positionLocked = true;
    //     lockedToTablePlane = false;
    //     positionOnLock = pos;
    // }

    public virtual void Update()
    {
        Vector3 target;
        // Position in the middle of the object (z is negative)
        if(selectedForLocking || this.arDefinition.IsGeneric())
        {
            if (TrackedObjects != null && TrackedObjects.Count == 1)
            {
                //target = TrackedObjects[0].position;
                target = (lockedToTablePlane ? NNPostProcessing.roundToCM(new Vector3(TrackedObjects[0].position.x, 0, TrackedObjects[0].position.z)) : NNPostProcessing.roundToCM(TrackedObjects[0].position));
                Quaternion rotationTarget = TrackedObjects[0].rotation;
                transform.localRotation = QuaternionUtil.SmoothDamp(transform.localRotation, rotationTarget, ref currentQuaternionVelocity, smoothTime); 
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, target, ref currentVelocity, smoothTime);
            }
        }else if(!hasBeenLocked && _defaultSlot != null)
        {
            target = _defaultSlot.position;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, target, ref currentVelocity, smoothTime);
        }
    }

    public ArDefinitionType GetArDefinitionType()
    {
        return this.arDefinition.arDefinitionType;
    }

    public Condition GetCondition()
    {
        return this.arDefinition.condition;
    }
}