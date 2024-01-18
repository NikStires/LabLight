using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

/// <summary>
/// Copied from the FollowMeToggle, but with reversed focus
/// Autofollow only when the user wants it, but most of the time the user will probably want to enable pinning to a certain location.
/// This script will signal when pinning occurs and the goal is to allow persistence of the pinned location.
/// </summary>
[RequireComponent(typeof(RadialView))]
[AddComponentMenu("Scripts/MRTK/SDK/PinningToggle")]
public class PinningToggle : PersistentLocation
{
    [SerializeField]
    private bool PinnedLocationPersistence;

    /// <summary>
    /// An optional Interactable to select/deselect when toggling the follow behavior.
    /// </summary>
    public Interactable InteractableObject
    {
        get { return interactableObject; }
        set { interactableObject = value; }
    }

    [SerializeField]
    [Tooltip("An optional Interactable to select/deselect when toggling the follow behavior.")]
    private Interactable interactableObject = null;

    private RadialView radialView = null;

    #region MonoBehaviour Implementation

    private void Awake()
    {
        radialView = GetComponent<RadialView>();
    }

    private void OnEnable()
    {
        if (PinnedLocationPersistence)
        {
            LoadLocation();
        }
    }

    #endregion MonoBehaviour Implementation

    /// <summary>
    /// Toggles the current follow behavior of the solver.
    /// </summary>
    public void TogglePinningBehaviour()
    {
        if (radialView != null)
        {
            SetPinning(radialView.enabled);
        }
    }

    /// <summary>
    /// Enables or disables the solver based on the pinned parameter.
    /// </summary>
    /// <param name="pinned">True if the solver should be active.</param>
    public void SetPinning(bool pinned)
    {
        if (radialView != null)
        {
            // Toggle Radial Solver component
            // You can tweak the detailed positioning behavior such as offset, lerping time, orientation type in the Inspector panel
            radialView.enabled = !pinned;

            if (interactableObject != null)
            {
                interactableObject.IsToggled = pinned;
            }

            if (PinnedLocationPersistence)
            {
                SaveLocation();
            }
        }
    }
}
