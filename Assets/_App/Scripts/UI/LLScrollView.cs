using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LLScrollView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private RectTransform contentRectTransform;
    
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 0.01f;
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private bool enableVerticalScroll = true;
    [SerializeField] private bool enableHorizontalScroll = false;
    
    [Header("Layout Settings")]
    [SerializeField] private float contentSpacing = 5f;
    
    private Vector2 dragStartPosition;
    private Vector2 contentStartPosition;
    private Vector2 currentVelocity;
    private bool isDragging;
    private RectTransform viewportRectTransform;
    private LayoutGroup contentLayoutGroup;
    private ContentSizeFitter contentSizeFitter;

    private void Awake()
    {
        InitializeComponents();
        ConfigureLayoutGroup();
        ConfigureContentSizeFitter();
        ConfigureContentTransform();
    }

    private void InitializeComponents()
    {
        viewportRectTransform = GetComponent<RectTransform>();
        EnsureRequiredComponents();
    }

    private void EnsureRequiredComponents()
    {
        var canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        var mask = GetComponent<Mask>() ?? gameObject.AddComponent<Mask>();
        
        if (!GetComponent<Image>())
        {
            var image = gameObject.AddComponent<Image>();
            image.color = Color.white;
            image.raycastTarget = true;
        }
    }

    private void ConfigureLayoutGroup()
    {
        contentLayoutGroup = contentRectTransform.GetComponent<LayoutGroup>();
        if (contentLayoutGroup == null)
        {
            contentLayoutGroup = contentRectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        if (contentLayoutGroup is VerticalLayoutGroup verticalLayout)
        {
            verticalLayout.childAlignment = TextAnchor.UpperCenter;
            verticalLayout.spacing = contentSpacing;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
        }
    }

    private void ConfigureContentSizeFitter()
    {
        contentSizeFitter = contentRectTransform.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = contentRectTransform.gameObject.AddComponent<ContentSizeFitter>();
        }
        
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    private void ConfigureContentTransform()
    {
        contentRectTransform.anchorMin = new Vector2(0, 1);
        contentRectTransform.anchorMax = new Vector2(1, 1);
        contentRectTransform.pivot = new Vector2(0.5f, 1f);
        contentRectTransform.sizeDelta = new Vector2(0, 0);
    }

    private void LateUpdate()
    {
        if (!isDragging)
        {
            ApplySmoothScrolling();
        }
    }

    private void ApplySmoothScrolling()
    {
        Vector2 targetPosition = contentRectTransform.anchoredPosition;
        Vector2 clampedPosition = ClampScrollPosition(targetPosition);

        contentRectTransform.anchoredPosition = Vector2.SmoothDamp(
            contentRectTransform.anchoredPosition,
            clampedPosition,
            ref currentVelocity,
            smoothTime
        );
    }

    private Vector2 ClampScrollPosition(Vector2 position)
    {
        float maxY = Mathf.Max(0, contentRectTransform.rect.height - viewportRectTransform.rect.height);
        float minX = Mathf.Min(0, -(contentRectTransform.rect.width - viewportRectTransform.rect.width));
        
        return new Vector2(
            Mathf.Clamp(position.x, minX, 0),
            Mathf.Clamp(position.y, 0, maxY)
        );
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        dragStartPosition = eventData.position;
        contentStartPosition = contentRectTransform.anchoredPosition;
        currentVelocity = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 difference = (eventData.position - dragStartPosition) * scrollSpeed;
        Vector2 targetPosition = contentStartPosition;

        if (enableVerticalScroll)
            targetPosition.y += difference.y;
        if (enableHorizontalScroll)
            targetPosition.x += difference.x;

        contentRectTransform.anchoredPosition = ClampScrollPosition(targetPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }
}