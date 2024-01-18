using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ApplicationVersionBehavior : MonoBehaviour
{
    private TextMeshProUGUI txtVersion;
    private string origText;
    [SerializeField]
    private bool isServerVersion = false;
    [SerializeField]
    private bool isClientVersion = false;

    private void Awake()
    {
        txtVersion = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        origText = txtVersion?.text;
        if (isClientVersion && txtVersion != null)
        {
            txtVersion.text = $"Client: {Application.version}";
        }
    }

    private ILighthouseControl lightHouseControl;
    private void OnEnable()
    {
        lightHouseControl = ServiceRegistry.GetService<ILighthouseControl>();
    }

    private void Update()
    {
        if (isServerVersion && txtVersion != null)
        {
            if (txtVersion.text == origText && !string.IsNullOrEmpty(lightHouseControl.GetBuildVersion()))
            {
                txtVersion.text = "Server: " + lightHouseControl.GetBuildVersion();
            }
        }
    }
}
