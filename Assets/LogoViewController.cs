using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoViewController : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadMenuSceneAfterAnimation());
    }

    private IEnumerator LoadMenuSceneAfterAnimation()
    {
        yield return new WaitForSeconds(3.0f);
        SceneLoader.Instance.LoadNewScene("ProtocolMenu");
    }
}
