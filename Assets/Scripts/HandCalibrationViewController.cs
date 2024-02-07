using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCalibrationViewController : MonoBehaviour
{
    [SerializeField] MeshRenderer progressRing;
    [SerializeField] MeshRenderer thumbTip;
    [SerializeField] MeshRenderer pointerTip;
    [SerializeField] MeshRenderer middleTip;
    [SerializeField] MeshRenderer ringTip;
    [SerializeField] MeshRenderer pinkyTip;
    [SerializeField] GameObject origin;

    [SerializeField] Material fillMaterial;

    private float progress = -0.4f;
    private float lerpDuration = 3f;

    private void Start()
    {
        fillMaterial.SetFloat("_FillRate", -0.4f);
        StartCoroutine(CalibrationAnimation());
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
