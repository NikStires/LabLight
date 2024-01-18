using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageIPAddressBehavior : MonoBehaviour
{
    private TouchScreenKeyboard _keyboard;
    public TMPro.TextMeshProUGUI _visibleText;
    // Start is called before the first frame update
    void Start()
    {
        _visibleText.text = Config.Hostname;
    }

    // Update is called once per frame
    void Update()
    {
        if (_keyboard != null)
        {
            var keyboardText = _keyboard.text;
            _visibleText.text = _keyboard.text;
            Config.Hostname = keyboardText;
        }
    }

    public void OpenKeyboard()
    {
        var origText = "";
        _keyboard = TouchScreenKeyboard.Open(origText, TouchScreenKeyboardType.NumberPad, false, false, false, false);
    }
}
