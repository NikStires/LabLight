using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// There is a bug with layout groups
// It looks like layout is not being performed before children have figured out their size
// Wait a frame, and force a layout of this component
public class LayoutBugFix : MonoBehaviour {
  void Start() {
    StartCoroutine(WaitFrame());
  }

  IEnumerator WaitFrame() {
    yield return 0;
    LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
  }
}