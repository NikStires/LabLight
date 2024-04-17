using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(LazyFollow))]
public class LLBasePanel : MonoBehaviour
{
    [SerializeField] GameObject _pinButton;
    XRSimpleInteractable _pinButtonInteractable;
    MeshRenderer _pinButtonMeshRenderer;

    [SerializeField] Material _pinnedMaterial;
    [SerializeField] Material _unpinnedMaterial;

    LazyFollow _lazyFollow;

    // Start is called before the first frame update
    public void Awake()
    {
        _lazyFollow = GetComponent<LazyFollow>();
        _pinButtonInteractable = _pinButton.GetComponent<XRSimpleInteractable>();
        _pinButtonMeshRenderer = _pinButton.GetComponent<MeshRenderer>();

        _pinButtonInteractable.selectEntered.AddListener(_ => OnPinButtonPressed());
    }

    void OnPinButtonPressed()
    {
        if(_lazyFollow.positionFollowMode == LazyFollow.PositionFollowMode.Follow)
        {
            Pin();
        }
        else
        {
            Unpin();
        }
    }

    public void Pin()
    {
        _lazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.None;
        _pinButtonMeshRenderer.material = _pinnedMaterial;
    }

    public void Unpin()
    {
        _lazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.Follow;
        _pinButtonMeshRenderer.material = _unpinnedMaterial;
    }
}
