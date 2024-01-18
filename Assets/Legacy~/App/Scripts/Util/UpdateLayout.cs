using UnityEngine;
using UnityEngine.UI;

// Force UI layout update 
public class UpdateLayout : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Number of time to force a rebuild, 0 = infinite")]
    private int RebuildCount;
    private int _currentCount;

    private void OnEnable()
    {
        _currentCount = 0;
    }

    private void LateUpdate()
    {
        if (RebuildCount == 0 || _currentCount < RebuildCount)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            _currentCount++;
        }
    }
}
