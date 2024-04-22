using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlaneInteraction : MonoBehaviour
{
    public PlaneInteractionManagerScriptableObject planeManager;

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
}
