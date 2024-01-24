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
    [SerializeField] MeshRenderer origin;

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
        yield return new WaitForSeconds(1f);
        StartCoroutine(LerpRingScale());
    }

    private IEnumerator LerpRingScale()
    {
        float timeElapsed = 0;
        origin.gameObject.SetActive(true);
        while (timeElapsed < lerpDuration)
        {
            progressRing.transform.localScale = progressRing.transform.localScale * Mathf.Lerp(1f, 0f, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        progressRing.transform.localScale = new Vector3(0,0,0);
        progressRing.gameObject.SetActive(false);
    }
}
