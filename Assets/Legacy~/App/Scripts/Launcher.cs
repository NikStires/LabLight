using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class Launcher : MonoBehaviour
{
  void Awake() {
    // StartCoroutine(WaitForLoad());
    SceneManager.LoadScene("Main");

    // var app = rootObjects.Find(o => o.name == "App");
    // Debug.Log(app.GetComponent<MainDriver>());
  }

  IEnumerator WaitForLoad() {
    Debug.Log("1");
    yield return null;
    Debug.Log("2");
    yield return null;
    Debug.Log("3");
    var scene = SceneManager.GetSceneByName("Main");
		var rootObjects = new List<GameObject>(scene.GetRootGameObjects());
    Debug.Log("Count " + rootObjects.Count);
    rootObjects.ForEach(o => Debug.Log(o.name));
  }
}