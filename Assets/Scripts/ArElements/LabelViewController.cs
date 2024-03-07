using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class LabelViewController : MonoBehaviour
{
    private Vector3 currentVelocity;
    private Quaternion currentQuaternionVelocity;
    private float smoothTime = .1f;

    private Vector3 _defaultPosition = Vector3.zero;
    private Quaternion _defaultOrientation = Quaternion.identity;

    public TextMeshProUGUI labelText;


    public void Initialize(List<TrackedObject> trackedObjects)
    {
        Debug.Log("initializing label controller");
        foreach(var trackedObject in trackedObjects)
        {
            TrackedObjects.Add(trackedObject);
        }

        //go to initial position

        if(TrackedObjects != null)
        {
            if(TrackedObjects.Count == 1)
            {
                Debug.Log("setting initial position to " + this.TrackedObjects[0].position);
                transform.localPosition = this.TrackedObjects[0].position;
                transform.localRotation = this.TrackedObjects[0].rotation;
                labelText.text = TrackedObjects[0].label;
            }
        }
        else
        {
            transform.position = _defaultPosition;
            transform.localRotation = _defaultOrientation;
        }
    }

    public void Update()
    {
        Vector3 target;
        if(TrackedObjects != null && TrackedObjects.Count == 1)
        {
            Debug.Log("Updating " + TrackedObjects[0].id  + TrackedObjects[0].position);
            target = NNPostProcessing.roundToCM(TrackedObjects[0].position);
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, target, ref currentVelocity, smoothTime);
            transform.localRotation = QuaternionUtil.SmoothDamp(transform.localRotation, TrackedObjects[0].rotation, ref currentQuaternionVelocity, smoothTime);
        }
        // else
        // {
        //     target = _defaultPosition;
        //     transform.localPosition = Vector3.SmoothDamp(transform.localPosition, target, ref currentVelocity, smoothTime);
        // }
    }

    public ReactiveCollection<TrackedObject> TrackedObjects = new ReactiveCollection<TrackedObject>();
}
