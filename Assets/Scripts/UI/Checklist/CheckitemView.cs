using System;
using UnityEngine;
using TMPro;
using UniRx;

public class CheckitemView : MonoBehaviour
{
    public bool itemChecked = false;

    [SerializeField] private Transform m_CheckMark;

    [SerializeField] private TextMeshProUGUI text;
    string rawText;
    string strikeText;

    [SerializeField] MeshRenderer m_backgroundMesh;
    [SerializeField] MeshRenderer m_ToggleMesh;

    [SerializeField] private Material checkedMaterial;
    [SerializeField] private Material uncheckedMaterial;

    protected Vector3 m_ToggleOnTargetPosition;
    protected Vector3 m_ToggleOffTargetPosition;

    float m_StartLerpTime;

    public float toggleOnPosition;
    public float toggleOffPosition;   
    public float lerpSpeed;

    private IDisposable subscription;

    void Start()
    {
        rawText = text.text;
        strikeText = "<s>" + text.text + "</s>";
        m_ToggleMesh.material = uncheckedMaterial;
        var meshPosition = m_ToggleMesh.transform.localPosition;
        m_ToggleOnTargetPosition = new Vector3(meshPosition.x, meshPosition.y, toggleOnPosition);
        m_ToggleOffTargetPosition = new Vector3(meshPosition.x, meshPosition.y, toggleOffPosition);
        m_ToggleMesh.transform.localPosition = m_ToggleOffTargetPosition;
    }

    public void Check()
    {
        itemChecked = true;
        text.text = strikeText;
        m_StartLerpTime = Time.time;
        m_ToggleMesh.material = checkedMaterial;
    }

    public void Uncheck()
    {
        itemChecked = false;
        text.text = rawText;
        m_StartLerpTime = Time.time;
        m_ToggleMesh.material = uncheckedMaterial;
    }

    public void SetAsActiveItem()
    {
        m_backgroundMesh.material = checkedMaterial;
    }

    public void SetAsInactiveItem()
    {
        m_backgroundMesh.material = uncheckedMaterial;
    }

    void Update()
    {
        var coveredAmount = (Time.time - m_StartLerpTime) * lerpSpeed;
        var lerpPercentage = coveredAmount / (toggleOffPosition * 2);
        m_ToggleMesh.transform.localPosition = Vector3.Lerp(itemChecked ? m_ToggleOffTargetPosition : m_ToggleOnTargetPosition, itemChecked ? m_ToggleOnTargetPosition : m_ToggleOffTargetPosition, lerpPercentage);
        if(!itemChecked)
        {
            m_CheckMark.gameObject.SetActive(false);
        }
        else
        {
            m_CheckMark.gameObject.SetActive(true);
        }
    }

    public void InitalizeCheckItem(ProtocolState.CheckItemState checkItem)
    {
        text.text = checkItem.Text;
        rawText = checkItem.Text;
        strikeText = "<s>" + checkItem.Text + "</s>";

        if(subscription != null)
        {
            subscription.Dispose();
        }

        subscription = checkItem.IsChecked.Subscribe(itemChecked => {
            if(itemChecked)
            {
                Check();
            }
            else
            {
                Uncheck();
            }
        });
    }
}
