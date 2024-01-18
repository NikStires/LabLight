using UnityEngine;

public class DelayedHide : MonoBehaviour
{
    [SerializeField]
    private float HideAfterSeconds = 5;

    private void OnEnable()
    {
        ShowChildren(true);
        Invoke("HideChildren", HideAfterSeconds);
    }

    private void HideChildren()
    {
        ShowChildren(false);
    }

    private void ShowChildren(bool show)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(show);
        }
    }
}
