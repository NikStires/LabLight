using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


public class PopupPanelViewController : MonoBehaviour
{
    //events are passed through a scriptable object to decrease coupling between classes
    //different popup panels can use different scriptable objects to communicate with other classes
    //listening classes simple need to subscribe to the events on the associated scriptable object
    [SerializeField] PopupEventSO popupEventSO;

    [SerializeField] XRSimpleInteractable yesButton;
    [SerializeField] XRSimpleInteractable noButton;
    
    // Start is called before the first frame update
    void Start()
    {
        yesButton.selectEntered.AddListener(_ => 
        {
            transform.GetChild(0).transform.gameObject.SetActive(false);
            popupEventSO.Yes();
        });
        
        noButton.selectEntered.AddListener(_ => 
        {
            transform.GetChild(0).transform.gameObject.SetActive(false);
            popupEventSO.No();
        });

        popupEventSO.OpenPopup.AddListener(() => transform.GetChild(0).transform.gameObject.SetActive(true));
    }
}
