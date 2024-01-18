using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
//using UnityEngine.UI.ProceduralImage;
using UnityEngine.Events;
using TMPro;

public class UiTile : MonoBehaviour, IMixedRealityFocusHandler, IMixedRealityPointerHandler, IMixedRealityTouchHandler
{
    public TextMeshPro title;
    public TextMeshPro description;
    //public ProceduralImage image;
    public RawImage microphone;

    public UnityEvent OnClick = new UnityEvent();
    public UnityEvent OnHold = new UnityEvent();

    private bool pointerDown;
    private float pointerDownTimer;
    public float requiredHoldTime;
    public Image fillImage;

    static readonly Color hiddenWhite = new Color32(235, 235, 235, 0);
    static readonly Color white = new Color32(235, 235, 235, 255);
    // static readonly Color pureWhite = new Color32(255, 255, 255, 255);
    static readonly Color textToColor = new Color32(30, 30, 30, 255);
    static readonly Color chillWhite = new Color32(235, 235, 235, 170);
    static readonly Color hidden = new Color32(30, 30, 30, 0);
    const float animationTime = .25f;

    public string Title
    {
        set
        {
            //color=#04a5f0
            title.text = "<u>" + value;
        }
    }

    public Vector2 Size
    {
        get
        {
            return new Vector2(380, 180);
        }
    }

    //LTDescr colorTo(TextMeshProUGUI text, Color colorTo, float duration)
    //{
    //    return LeanTween.value(text.gameObject, text.color, colorTo, duration).setOnUpdate((Color val) =>
    //    {
    //        text.color = val;
    //    });
    //}

    //LTDescr colorTo(Text text, Color colorTo, float duration)
    //{
    //    return LeanTween.value(text.gameObject, text.color, colorTo, duration).setOnUpdate((Color val) =>
    //    {
    //        text.color = val;
    //    });
    //}

    //LTDescr colorTo(RawImage img, Color colorTo, float duration)
    //{
    //    return LeanTween.value(img.gameObject, img.color, colorTo, duration).setOnUpdate((Color val) =>
    //    {
    //        img.color = val;
    //    });
    //}

    //LTDescr colorTo(ProceduralImage img, Color colorTo, float duration)
    //{
    //    return LeanTween.value(img.gameObject, img.color, colorTo, duration).setOnUpdate((Color val) =>
    //    {
    //        img.color = val;
    //    });
    //}

    public void Enter(float delay)
    {
        //var duration = 1.5f;

        var pos = gameObject.transform.localPosition;
        gameObject.transform.localPosition = pos + new Vector3(50, 0, 0);
        //image.color = hiddenWhite;
        // title.color = hiddenWhite;
        //description.color = hiddenWhite;
        //microphone.color = hiddenWhite;

        //LeanTween.moveLocalX(gameObject, pos.x, duration).setEaseOutQuint().setDelay(delay);
        //// colorTo(title, white, .3f).setDelay(delay);
        //colorTo(microphone, white, .3f).setDelay(delay);
        //colorTo(description, chillWhite, .3f).setDelay(.15f + delay).setOnComplete(_ =>
        //{
        //    enabled = true;
        //});
    }

    // public void Exit(float delay) {
    //   enabled = false;
    //   LeanTween.cancel(gameObject);
    //   var duration = .3f;
    //   var pos = gameObject.transform.localPosition.x - 10;
    //   LeanTween.moveLocalX(gameObject, pos, duration).setEaseOutQuint().setDelay(delay).setOnComplete(_ => {
    //     Destroy(this.gameObject);
    //   });
    //   colorTo(microphone, hiddenWhite, duration).setDelay(delay);
    //   // colorTo(title, hiddenWhite, duration).setEaseOutQuint().setDelay(delay);
    //   colorTo(description, hiddenWhite, duration).setEaseOutQuint().setDelay(delay);
    // }

    // public void ClickAnimation() {
    //   // AudioController.Instance.PlayClip();
    //   LeanTween.moveLocalZ(gameObject, -50, animationTime).setDelay(1).setEaseInQuint();
    //   LeanTween.value(gameObject, image.color, hiddenWhite, animationTime).setDelay(1).setOnUpdate((Color val) => { 
    //     image.color = val;
    //   }).setOnComplete(_ => {
    //     Destroy(this.gameObject);
    //   });
    //   // colorTo(gameObject, hiddenWhite, white, animationTime).setOnUpdate((Color val) => { 
    //   //   image.color = val;
    //   // });
    //   // colorTo(gameObject, white, textToColor, animationTime).setOnUpdate((Color val) => {
    //   //   title.color = val;
    //   // });
    //   // colorTo(gameObject, chillWhite, textToColor, animationTime).setOnUpdate((Color val) => {
    //   //   description.color = val;
    //   // });
    // }

    // public void Pause(float delay) {
    //   enabled = false;
    //   LeanTween.cancel(gameObject);
    //   var duration = .6f;
    //   var pos = gameObject.transform.localPosition.x - 10;
    //   LeanTween.moveLocalX(gameObject, pos, duration).setEaseOutQuint().setDelay(delay).setOnComplete(_ => {
    //     Destroy(this.gameObject);
    //   });
    //   colorTo(microphone, hiddenWhite, duration).setDelay(delay);
    //   // colorTo(title, hiddenWhite, duration).setEaseOutQuint().setDelay(delay);
    //   colorTo(description, hiddenWhite, duration).setEaseOutQuint().setDelay(delay);
    // }

    // public void Resume() {

    // }

    public void OnPointerDown(MixedRealityPointerEventData data) 
    {
    }
    public void OnPointerUp(MixedRealityPointerEventData data)
    {
        Reset();
    }
    public void OnPointerDragged(MixedRealityPointerEventData data) { }
    public void OnPointerClicked(MixedRealityPointerEventData data)
    {
    }

    public void OnTouchStarted(HandTrackingInputEventData eventData)
    {
        pointerDown = true;
    }
    public void OnTouchUpdated(HandTrackingInputEventData eventData) { }
    public void OnTouchCompleted(HandTrackingInputEventData eventData)
    {
        if (pointerDownTimer < requiredHoldTime)
        {
            OnClick.Invoke();
        }
        Reset();
    }

    void Reset()
    {
        pointerDown = false;
        pointerDownTimer = 0;
        fillImage.fillAmount = pointerDownTimer / requiredHoldTime;
        fillImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if(pointerDown)
        {
            pointerDownTimer += Time.deltaTime;
            if(pointerDownTimer >= requiredHoldTime)
            {
                OnHold.Invoke();
                Reset();
            }
            if (pointerDownTimer > 0.5f)
            {
                fillImage.fillAmount = pointerDownTimer / requiredHoldTime;
                fillImage.gameObject.SetActive(true);
            }
        }
    }

    public void OnFocusEnter(FocusEventData eventData)
    {
        // if (!enabled) return;

        // AudioController.Instance.PlayClip();
        //LeanTween.moveLocalZ(gameObject, -50, animationTime).setEaseInOutQuint();
        //LeanTween.value(gameObject, hiddenWhite, white, animationTime).setOnUpdate((Color val) =>
        //{
        //    image.color = val;
        //});
        // LeanTween.value(gameObject, white, textToColor, animationTime).setOnUpdate((Color val) => {
        //   title.color = val;
        // });
        //LeanTween.value(gameObject, chillWhite, textToColor, animationTime).setOnUpdate((Color val) =>
        //{
        //    description.color = val;
        //});
    }

    void ClearFocus()
    {
        //LeanTween.cancel(gameObject);
        //LeanTween.moveLocalZ(gameObject, 0, animationTime).setEaseInOutQuint();
        //LeanTween.value(gameObject, image.color, hiddenWhite, animationTime).setOnUpdate((Color val) =>
        //{
        //    image.color = val;
        //});
        // LeanTween.value(gameObject, title.color, white, animationTime).setOnUpdate((Color val) => { 
        //   title.color = val;
        // });
        //LeanTween.value(gameObject, title.color, chillWhite, animationTime).setOnUpdate((Color val) =>
        //{
        //    description.color = val;
        //});
    }

    public void OnFocusExit(FocusEventData eventData)
    {
        ClearFocus();
    }

    void OnDisable()
    {
        ClearFocus();
    }
}