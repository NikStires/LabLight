using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "PopupEventSO", menuName = "ScriptableObjects/PopupEvent", order = 1)]
public class PopupEventSO : ScriptableObject
{
    [SerializeField] public UnityEvent OnYesButtonPressed = new UnityEvent();
    [SerializeField] public UnityEvent OnNoButtonPressed = new UnityEvent();
    [SerializeField] public UnityEvent OpenPopup = new UnityEvent();
    // Start is called before the first frame update
    void Start()
    {
        if(OnYesButtonPressed == null)
        {
            OnYesButtonPressed = new UnityEvent();
        }
        if(OnNoButtonPressed == null)
        {
            OnNoButtonPressed = new UnityEvent();
        }
        if(OpenPopup == null)
        {
            OpenPopup = new UnityEvent();
        }
    }

    public void Yes()
    {
        OnYesButtonPressed.Invoke();
    }

    public void No()
    {
        OnNoButtonPressed.Invoke();
    }

    public void Open()
    {
        OpenPopup.Invoke();
    }
}
