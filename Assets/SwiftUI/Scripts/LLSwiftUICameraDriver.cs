using UnityEngine;
using System.Runtime.InteropServices;

public class LLSwiftUICameraDriver : MonoBehaviour
{

    [DllImport("__Internal")]
    private static extern void OpenMainCameraWindow();

    public void OnMainCameraButtonClick()
    {
        OpenMainCameraWindow();
    }
}
