using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Unity.PolySpatial.InputDevices;

public class SpatialUIInputManager : MonoBehaviour
{
    [SerializeField]
    InputActionReference m_Touch;
    [SerializeField]
    InputActionReference m_HMDpos;

    PanelTransformBehavior m_CurrentSelection;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        m_Touch.action.Enable();
        m_HMDpos.action.Enable();
    }

    void Update()
    {
        var touchData = m_Touch.action.ReadValue<SpatialPointerState>();
        var hmdPositionData = m_HMDpos.action.ReadValue<Vector3>();

        var activeTouches = Touch.activeTouches;
        if (activeTouches.Count > 0)
        {
            //get primary touch data
            var primaryTouchData = EnhancedSpatialPointerSupport.GetPointerState(activeTouches[0]);
            //get touch phase
            var primaryTouchPhase = activeTouches[0].phase;
            //get target object
            var target = touchData.targetObject;

            if (primaryTouchPhase == TouchPhase.Began)
            {
                Debug.Log("Touch Begin");
                if (target != null)
                {
                    if (target.TryGetComponent(out SpatialUI button))
                    {
                        button.Press(touchData.interactionPosition);
                    }
                    else if(target.TryGetComponent(out PanelTransformBehavior panelTransform))
                    {
                        m_CurrentSelection = panelTransform;
                        m_CurrentSelection.Select();
                    }
                }
            }
            // case for dragging
            if (primaryTouchPhase == TouchPhase.Moved)
            {
                if (target != null)
                {
                    if (target.TryGetComponent(out SpatialUISlider slider))
                    {
                        slider.Press(touchData.interactionPosition);
                    }
                    else if (m_CurrentSelection != null)
                    {
                        m_CurrentSelection.Drag(primaryTouchData.interactionPosition, hmdPositionData);
                    }
                }
            }
            if(primaryTouchPhase == TouchPhase.Ended || primaryTouchPhase == TouchPhase.Canceled)
            {
                Debug.Log("Touch End"); 
                if (m_CurrentSelection != null)
                {
                    m_CurrentSelection.Deselect();
                    m_CurrentSelection = null;
                }
            }
        }
    }
}
