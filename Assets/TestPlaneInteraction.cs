using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlaneInteraction : MonoBehaviour
{
    public GameObject jointPrefab;

    private GameObject joint;
    private Transform sphere;
    public PlaneInteractionManagerScriptableObject planeManager;

    public bool testFingerJoint;

    public enum testType
    {
        testHeadPlacement,
        testTapToPlace,
        testCollision,
        none
    }

    public testType currentTest;

    public void OnEnable()
    {
        switch(currentTest)
        {
            case testType.testHeadPlacement:
                StartCoroutine(testHeadPlacement());
                break;
            case testType.testTapToPlace:
                StartCoroutine(testTapToPlace());
                break;
            case testType.testCollision:
                StartCoroutine(testCollision());
                break;
            case testType.none:
                break;
        }
        if(testFingerJoint)
        {
            joint = Instantiate(jointPrefab, Camera.main.transform);
            sphere = joint.transform.GetChild(0);
        }
    }

    private IEnumerator testHeadPlacement()
    {
        yield return new WaitForSeconds(5);
        planeManager.OnEnableHeadPlacement();
    }

    private IEnumerator testTapToPlace()
    {
        yield return new WaitForSeconds(5);
        planeManager.OnEnableTapToPlace();
    }

    private IEnumerator testCollision()
    {
        yield return new WaitForSeconds(5);
        planeManager.OnFingerTipPlaneCollision(new Vector3(0, 0, 0));
    }

    public void Update()
    {
        if(testFingerJoint)
        {
            sphere.position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
        }
    }
}
