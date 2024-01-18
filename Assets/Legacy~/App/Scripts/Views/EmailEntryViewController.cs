using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class EmailEntryViewController : MonoBehaviour
{
    public Interactable addEmailButton;
    public Interactable sendEmailButton;
    public Transform emailListUI;
    public Transform emailItem;
    public TouchScreenKeyboard keyboard;
    public Transform inputField;
    public Transform inputFieldText;

    private List<string> recipients = new List<string>();

    void OnEnable()
    {
        addEmailButton.OnClick.AsObservable().Subscribe(_ =>
        {
            GetComponent<MixedRealityKeyboard>().ShowKeyboard("", false);
            inputField.GetComponent<MRTKTMPInputField>().ActivateInputField();
        }).AddTo(this);

        sendEmailButton.OnClick.AsObservable().Subscribe(_ => 
        {
            // Mailservice is stateless so we give it all the info needed for sending the mail
            ServiceRegistry.GetService<IMailService>().SendMessage(recipients, "Lablight AR checklist csv file", "Below is attached checklist ready for sign off", "lablightForm.csv", "");
            this.gameObject.SetActive(false);
        }).AddTo(this);

        foreach (string recipient in recipients)
        {
            createEmailItem(recipient);
        }
    }

    public void createEmailItem(string recipient)
    {
        //create UI element above input field
        var email = Instantiate(emailItem, emailListUI);
        email.SetSiblingIndex(emailListUI.childCount - 2);

        email.GetChild(0).GetComponent<TextMeshProUGUI>().text = recipient;
        email.GetChild(1).GetComponent<Interactable>().OnClick.AsObservable().Subscribe(_ =>
        {
            recipients.Remove(recipient);
            Destroy(email.GetComponent<HorizontalLayoutGroup>());
            Destroy(email.gameObject);
        });
    }

    public void addEmail()
    {
        //get text entry
        string recipient = inputFieldText.GetComponent<TextMeshProUGUI>().text;
        //remove blank space inserted at the end for some awesome reason
        recipient = recipient.Substring(0, recipient.Length - 1);
        inputFieldText.GetComponent<TextMeshProUGUI>().text = "";

        if (recipient.IndexOf('@') > 0)
        {
            //add email to recipients list
            recipients.Add(recipient);
            //render recipient
            createEmailItem(recipient);
        }
    }
}
