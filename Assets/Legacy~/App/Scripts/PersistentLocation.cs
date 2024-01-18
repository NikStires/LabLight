using UnityEngine;

/// <summary>
/// Storage and retrieval of position and orientation
/// </summary>
public class PersistentLocation : MonoBehaviour
{
    [SerializeField]
    private string StoragePrefix = string.Empty;

    private void OnEnable()
    {
        LoadLocation();
    }

    public void LoadLocation()
    {
        if (PlayerPrefs.HasKey(StoragePrefix + "X") && PlayerPrefs.HasKey(StoragePrefix + "Y") && PlayerPrefs.HasKey(StoragePrefix + "Z"))
        {
            transform.localPosition = new Vector3(PlayerPrefs.GetFloat(StoragePrefix + "X"),
                                                    PlayerPrefs.GetFloat(StoragePrefix + "Y"),
                                                    PlayerPrefs.GetFloat(StoragePrefix + "Z"));
        }

        if (PlayerPrefs.HasKey(StoragePrefix + "RotX") && PlayerPrefs.HasKey(StoragePrefix + "RotY") && PlayerPrefs.HasKey(StoragePrefix + "RotZ") && PlayerPrefs.HasKey(StoragePrefix + "RotW"))
        {
            transform.localRotation = new Quaternion(   PlayerPrefs.GetFloat(StoragePrefix + "RotX"),
                                                        PlayerPrefs.GetFloat(StoragePrefix + "RotY"),
                                                        PlayerPrefs.GetFloat(StoragePrefix + "RotZ"),
                                                        PlayerPrefs.GetFloat(StoragePrefix + "RotW"));
        }

        if (PlayerPrefs.HasKey(StoragePrefix + "ScaleX") && PlayerPrefs.HasKey(StoragePrefix + "ScaleY") && PlayerPrefs.HasKey(StoragePrefix + "ScaleZ"))
        {
            transform.localScale = new Vector3(PlayerPrefs.GetFloat(StoragePrefix + "ScaleX"),
                                                    PlayerPrefs.GetFloat(StoragePrefix + "ScaleY"),
                                                    PlayerPrefs.GetFloat(StoragePrefix + "ScaleZ"));
        }
    }

    public void SaveLocation()
    {
        PlayerPrefs.SetFloat(StoragePrefix + "X", transform.localPosition.x);
        PlayerPrefs.SetFloat(StoragePrefix + "Y", transform.localPosition.y);
        PlayerPrefs.SetFloat(StoragePrefix + "Z", transform.localPosition.z);

        PlayerPrefs.SetFloat(StoragePrefix + "RotX", transform.localRotation.x);
        PlayerPrefs.SetFloat(StoragePrefix + "RotY", transform.localRotation.y);
        PlayerPrefs.SetFloat(StoragePrefix + "RotZ", transform.localRotation.z);
        PlayerPrefs.SetFloat(StoragePrefix + "RotW", transform.localRotation.w);

        PlayerPrefs.SetFloat(StoragePrefix + "ScaleX", transform.localScale.x);
        PlayerPrefs.SetFloat(StoragePrefix + "ScaleY", transform.localScale.y);
        PlayerPrefs.SetFloat(StoragePrefix + "ScaleZ", transform.localScale.z);

        PlayerPrefs.Save();
    }
}
