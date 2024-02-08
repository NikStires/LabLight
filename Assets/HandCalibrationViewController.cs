using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.ARFoundation;

public class HandCalibrationViewController : MonoBehaviour
{
    [SerializeField] MeshRenderer progressRing;
    [SerializeField] MeshRenderer thumbTip;
    [SerializeField] MeshRenderer pointerTip;
    [SerializeField] MeshRenderer middleTip;
    [SerializeField] MeshRenderer ringTip;
    [SerializeField] MeshRenderer pinkyTip;
    [SerializeField] GameObject origin;

    [SerializeField] GameObject jointPrefab;

    [SerializeField] Material fillMaterial;

    [SerializeField] GameObject tapToPlacePrefab;

    XRHandSubsystem m_HandSubsystem;

    Dictionary<XRHandJointID, GameObject> jointsRight = new();
    Dictionary<XRHandJointID, GameObject> jointsLeft = new();

    ARPlaneManager planeManager;

    private float progress = -0.4f;
    private float lerpDuration = 3f;

    private void Start()
    {
        fillMaterial.SetFloat("_FillRate", -0.4f);
        //StartCoroutine(CalibrationAnimation());
        var handSubsystems = new List<XRHandSubsystem>();
        var planeManager = GetComponent<ARPlaneManager>();
        SubsystemManager.GetSubsystems(handSubsystems);

        for (var i = 0; i < handSubsystems.Count; ++i)
        {
            var handSubsystem = handSubsystems[i];
            if (handSubsystem.running)
            {
                m_HandSubsystem = handSubsystem;
                break;
            }
        }

        // if (m_HandSubsystem != null)
        //     m_HandSubsystem.updatedHands += OnUpdatedHands;
    }



    void OnInteractableEnter()
    {
        //Pose pointerFingerPose = new Pose(origin.position, origin.rotation);
        XRHandJoint trackingData = m_HandSubsystem.rightHand.GetJoint(XRHandJointIDUtility.FromIndex((int)XRHandJointID.IndexTip));
        if(trackingData.TryGetPose(out Pose pose))
        {
            /*
            Pose xrOrigin = new Pose(planeManager.Transform.position, planeManager.Transform.rotation);
            pose = pose.GetTransformedBy(xrOrigin);*/
            var anchorManager = GetComponent<ARAnchorManager>();
            //ARAnchor anchor = anchorManager.AttachAnchor(plane, pose);
            //var instance = Instantiate(tapToPlacePrefab, pose.position, Quaternion.identity);
            //instance.transform.parent = anchor.transform;
        }
    }

    private IEnumerator CalibrationAnimation()
    {
        thumbTip.gameObject.SetActive(true);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        yield return new WaitForSeconds(1f);
        pointerTip.gameObject.SetActive(true);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        yield return new WaitForSeconds(1f);
        middleTip.gameObject.SetActive(true);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        yield return new WaitForSeconds(1f);
        ringTip.gameObject.SetActive(true);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        yield return new WaitForSeconds(1f);
        pinkyTip.gameObject.SetActive(true);
        progress += 0.14f;
        fillMaterial.SetFloat("_FillRate", progress);
        StartCoroutine(LerpRingScale());
    }

    private IEnumerator LerpRingScale()
    {
        float timeElapsed = 0;
        origin.SetActive(true);
        while (timeElapsed < lerpDuration)
        {
            progressRing.transform.localScale = progressRing.transform.localScale * Mathf.Lerp(1f, 0f, timeElapsed / lerpDuration);
            if(progressRing.transform.localScale.x < 0.22f)
            {
                StartCoroutine(DeactivateFingerPoints());
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        progressRing.transform.localScale = new Vector3(0,0,0);
        yield return new WaitForSeconds(3f);
        origin.SetActive(false);
    }

    private IEnumerator DeactivateFingerPoints()
    {
        yield return new WaitForSeconds(0.05f);
        progressRing.gameObject.SetActive(false);
        thumbTip.gameObject.SetActive(false);
        pointerTip.gameObject.SetActive(false);
        middleTip.gameObject.SetActive(false);
        ringTip.gameObject.SetActive(false);
        pinkyTip.gameObject.SetActive(false);
    }


}
