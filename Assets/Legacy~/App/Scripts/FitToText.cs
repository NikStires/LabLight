using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FitToText : MonoBehaviour
{

    public RectTransform Content;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector2(rectTransform.localPosition.x, Content.localPosition.y + 5f);
        rectTransform.sizeDelta = new Vector2(110, Content.rect.height);
    }
}
