using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class ProcedurePanelViewController : MonoBehaviour
{
    [Tooltip("Show/hide procedure panel")]
    public GameObject procedurePanel;
    public TextMeshProUGUI procedureTitle;
    public RadialView radialView;

    [Header("Reference Frames")]
    [Tooltip("Parent for ar objects in stage frame")]
    public Transform contentFrame;

    [Header("Content Item Prefabs")]
    [Tooltip("Placed in contentFrame canvas")]
    public LayoutController ContainerHorizontalItem;
    public LayoutController ContainerVerticalItem;
    public TextController TextItem;
    public PropertyTextController PropertyItem;
    public ImageController ImageItem;
    public VideoController VideoItem;
    public SoundController SoundItem;

    [Header("Scrolling Management")]
    public ScrollRect scrollRect;
    public GameObject viewPort;

    [SerializeField]
    private bool enableScrollbars = true;

    private Action disposeVoice;
    private IAudio audioPlayer;
    private bool pinned = false;

    private List<MonoBehaviour> contentItemInstances = new List<MonoBehaviour>();

    void Awake()
    {
        audioPlayer = ServiceRegistry.GetService<IAudio>();
        
        ProtocolState.procedureStream.Subscribe(procedureName => procedureTitle.text = procedureName).AddTo(this);
        
        ProtocolState.stepStream.Subscribe(_ => UpdateContentItems()).AddTo(this);

        ProtocolState.checklistStream.Subscribe(_ => UpdateContentItems()).AddTo(this);
    }

    void OnEnable()
    {
        UpdateContentItems();
        SetupVoiceCommands();
    }

    void OnDisable()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;
    }

    private void UpdateContentItems()
    {
        //remove all content item views
        foreach (Transform child in contentFrame)
        {
            var imageController = child.GetComponent<ImageController>();
            if (imageController)
            {
                contentItemInstances.Remove((MonoBehaviour)imageController);
                Destroy(imageController.gameObject);
                Destroy(child);
                continue;
            }
            var textController = child.GetComponent<TextController>();
            if(textController)
            {
                contentItemInstances.Remove((MonoBehaviour)textController);
                Destroy(textController.gameObject);
                Destroy(child);
            }
        }

        if(ProtocolState.procedureDef.Value != null)
        {
            //create content items for this step
            var currentStep = ProtocolState.procedureDef.Value.steps[ProtocolState.Step];

            //create content items for current check item, iif no content items for check item show step content items
            if(currentStep.checklist != null && currentStep.checklist[ProtocolState.CheckItem].contentItems.Count > 0)
            {
                CreateContentItem(currentStep.checklist[ProtocolState.CheckItem].contentItems, contentFrame.GetComponent<LayoutGroup>(), null);
            }
            else
            {
                CreateContentItem(currentStep.contentItems, contentFrame.GetComponent<LayoutGroup>(), null);
            }
        }
    }

     private void CreateContentItem(List<ContentItem> contentItems, LayoutGroup container, ContainerElementViewController containerController, bool store = true)
    {
        foreach (var contentItem in contentItems)
        {
            switch (contentItem.contentType)
            {
                case ContentType.Property:
                    PropertyTextController propertyTextController = Instantiate(PropertyItem, container.transform);
                    propertyTextController.ContentItem = contentItem as PropertyItem;
                    propertyTextController.ContainerController = containerController;

                    if (store)
                    {
                        contentItemInstances.Add(propertyTextController);
                    }
                    break;
                case ContentType.Text:
                    var textController = Instantiate(TextItem, container.transform);
                    textController.ContentItem = contentItem as TextItem;

                    if (store)
                    {
                        contentItemInstances.Add(textController);
                    }
                    break;
                case ContentType.Image:
                    var imageController = Instantiate(ImageItem, container.transform);
                    imageController.ContentItem = contentItem as ImageItem;

                    if (store)
                    {
                        contentItemInstances.Add(imageController);
                    }
                    break;
                case ContentType.Video:
                    var videoController = Instantiate(VideoItem, container.transform);
                    videoController.ContentItem = contentItem as VideoItem;

                    if (store)
                    {
                        contentItemInstances.Add(videoController);
                    }
                    break;
                case ContentType.Sound:
                    var soundController = Instantiate(SoundItem, container.transform);
                    soundController.ContentItem = contentItem as SoundItem;

                    if (store)
                    {
                        contentItemInstances.Add(soundController);
                    }
                    break;
                case ContentType.Layout:
                    // Recurse into subcontainers and their items
                    LayoutItem layoutItem = ((LayoutItem)contentItem);
                    var layoutController = Instantiate((layoutItem.layoutType == LayoutType.Vertical) ? ContainerVerticalItem : ContainerHorizontalItem, container.transform);
                    layoutController.ContentItem = contentItem as LayoutItem;

                    if (store)
                    {
                        contentItemInstances.Add(layoutController);
                    }
                    CreateContentItem(layoutItem.contentItems, layoutController.LayoutGroup, containerController);
                    break;
            }
        }
    }

    private void clearContentItems()
    {
        foreach (var contentItem in contentItemInstances)
        {
            Destroy(contentItem.gameObject);
        }
        contentItemInstances.Clear();
    }

    private void ConfigureScrollbars()
    {
        if (!enableScrollbars)
        {
            viewPort.transform.SetParent(scrollRect.transform.parent);
            RectTransform rectran = viewPort.GetComponent<RectTransform>();
            rectran.anchorMin = new Vector2(0, 0);
            rectran.anchorMax = new Vector2(1, 1);
            rectran.pivot = new Vector2(0.0f, 1.0f);

            scrollRect.gameObject.SetActive(false);
        }
    }

    private int clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    IEnumerator MoveIntoView(float duration)
    {
        float time = 0;
        Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.6f;

        var rotationEuler = Camera.main.transform.rotation.eulerAngles;
        this.transform.rotation = Quaternion.Euler(0, rotationEuler.y, 0);

        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }

        public void TogglePin()
    {
        if(pinned)
        {
            radialView.enabled = true;
            pinned = false;
        }
        else
        {
            radialView.enabled = false;
            pinned = true;
        }
    }

    void SetupVoiceCommands()
    {
                disposeVoice?.Invoke();
        disposeVoice = null;

        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(new Dictionary<string, Action>()
        {
            {"hide", () =>
                {
                    audioPlayer.Play(AudioEventEnum.Hidden);
                    procedurePanel.SetActive(false);
                }
            },
            {"show", () =>
                {
                    audioPlayer.Play(AudioEventEnum.Shown);
                    procedurePanel.SetActive(true);
                }
            },
            {"unhide", () =>
                {
                    audioPlayer.Play(AudioEventEnum.Shown);
                    procedurePanel.SetActive(true);
                }
            },
            {"fetch procedure screen", () =>
                {
                    audioPlayer.Play(AudioEventEnum.Fetch);
                    StartCoroutine(MoveIntoView(1f));
                }
            },
        });
    }
}
