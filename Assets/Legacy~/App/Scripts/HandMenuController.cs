using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.WorldLocking.Core;
using UnityEngine;
using UniRx;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class HandMenuController : MonoBehaviour
{
    public WorldLockingContext worldLockingObject;
    public DebugWindow DebugWindow;
    public Interactable lockingToggle;
    public Interactable debugWindowToggle;

    private HandConstraintPalmUp _handConstraintPalmUp;

    private void Start()
    {
        _handConstraintPalmUp = this.GetComponent<HandConstraintPalmUp>();

        SessionState.EnableHandMenu.Subscribe(value =>
        {
            _handConstraintPalmUp.enabled = value;
        }).AddTo(this);

#if UNITY_EDITOR
        // Disable flat hand requiremenent so we can test the handmenu in editor
        _handConstraintPalmUp.RequireFlatHand = false;
#endif
    }

    private void OnEnable()
    {
        //lockingToggle.IsToggled = worldLockingObject.Enabled;
        DebugWindow.WindowActiveChanged += DebugWindow_WindowActiveChanged;
        DebugWindow_WindowActiveChanged(this, null);
    }

    private void OnDisable()
    {
        if (DebugWindow != null)
        {
            DebugWindow.WindowActiveChanged -= DebugWindow_WindowActiveChanged;
        }
    }

    private void DebugWindow_WindowActiveChanged(object sender, System.EventArgs e)
    {
        debugWindowToggle.IsToggled = DebugWindow.gameObject.activeInHierarchy;
    }

    public void UpdateWorldLockingToggle()
    {
        //worldLockingObject.SetActive(lockingToggle.IsToggled);
    }

    public void ResetWorldLocking()
    {
        WorldLockingManager.GetInstance().Reset();
    }

    public void ClearWorldLocking()
    {
        WorldLockingManager.GetInstance().Reset();
        // Clean up persistence
        WorldLockingManager.GetInstance().Save();
    }

    public void UpdateDebugWindowToggle()
    {
        DebugWindow.gameObject.SetActive(debugWindowToggle.IsToggled);
    }

    public void ClearDebugWindow()
    {
        DebugWindow.Clear();
    }
}
