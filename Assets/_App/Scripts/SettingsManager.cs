using UnityEngine;
using UniRx;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Load from and save settings to playerprefs
/// </summary>
/// 
public class SettingsManager : MonoBehaviour
{

    public static SettingsManager instance;
    public SettingsManagerScriptableObject settingsManagerSO;

    public Dictionary<LablightSettings, string> settingKeys = new Dictionary<LablightSettings, string>(); //add to this dictionary to add new settings

    private Dictionary<LablightSettings, bool> settingsValues = new Dictionary<LablightSettings, bool>();

    private void Awake()
    {
        if(instance == null || instance == this)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
    void Start()
    {
        // Initialize from player preferences
        foreach(LablightSettings setting in LablightSettings.GetValues(typeof(LablightSettings))) //initialize dictionary with all settings in enum
        {
            settingKeys[setting] = setting.ToString(); //initialize string representation of settings -> used for player pref savings
            bool value = PlayerPrefs.GetInt(setting.ToString()) == 1;
            settingsValues[setting] = value; //initialize values from saved playerprefs
        }

        settingsManagerSO.settingChanged.AddListener(settingChanged =>
        {
            if(!settingsValues.ContainsKey(settingChanged.Item1))
            {
                Debug.Log("Setting not found: " + settingChanged.Item1.ToString());
                return;
            }
            settingsValues[settingChanged.Item1] = settingChanged.Item2;
            PlayerPrefs.SetInt(settingKeys[settingChanged.Item1], settingChanged.Item2 ? 1 : 0);
            PlayerPrefs.Save();
        });
    }

    public bool getSettingValue(LablightSettings setting)
    {
        if(!settingsValues.ContainsKey(setting))
        {
            Debug.Log("Setting not found: " + setting.ToString());
            return false; //default return false if setting not found
        }
        return settingsValues[setting];
    }
}
