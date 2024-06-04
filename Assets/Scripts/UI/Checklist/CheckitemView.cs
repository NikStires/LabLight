using System;
using UnityEngine;
using TMPro;
using UniRx;

public class CheckitemView : MonoBehaviour
{
    public bool itemChecked = false;

    [SerializeField] private TextMeshProUGUI text;
    string rawText;
    string strikeText;

    [SerializeField] Material activeMaterial;
    [SerializeField] Material defaultMaterial;
    [SerializeField] MeshRenderer m_backgroundMesh;

    private IDisposable subscription;

    void Start()
    {
        rawText = text.text;
        strikeText = "<s>" + text.text + "</s>";
    }

    public void Check()
    {
        itemChecked = true;
        text.text = strikeText;
    }

    public void Uncheck()
    {
        itemChecked = false;
        text.text = rawText;
    }

    public void SetAsActiveItem()
    {
        m_backgroundMesh.material = activeMaterial;
    }

    public void SetAsInactiveItem()
    {
        m_backgroundMesh.material = defaultMaterial;
    }

    public void InitalizeCheckItem(ProtocolState.CheckItemState checkItem)
    {
        //check puntcuation
        if(checkItem.Text != null)
        {
            var checkItemText = checkItem.Text = char.ToUpper(checkItem.Text[0]) + checkItem.Text.Substring(1);

            text.text = checkItemText;
            rawText = checkItemText;
            strikeText = "<s>" + checkItemText + "</s>";
        }

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
