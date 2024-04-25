using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;

public class NetworkingDebugViewController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Text;

    public void Log(string message)
    {
        Text.text = Text.text + "\n" + message;
    }

    public void ToggleView()
    {
        transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeSelf);
    }
}
