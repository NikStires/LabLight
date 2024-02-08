using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;

public class NetworkingDebugViewController : MonoBehaviour
{
    [SerializeField] Networking networking;
    [SerializeField] TextMeshProUGUI Text;

    public void Log(string message)
    {
        Text.text = Text.text + "\n" + message;
    }
}
