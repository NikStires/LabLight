using System;
using UniRx;
using UnityEngine;
/// <summary>
/// Controller for displaying the volumetric workspace intro animation
/// </summary>
public class IntroScreen : ScreenViewController
{
    public MeshFilter volumetricGrid;

    private void OnEnable()
    {
        if (volumetricGrid != null)
        {
            var gridMaterial = volumetricGrid.GetComponent<MeshRenderer>().material;

            LeanTween.value(volumetricGrid.gameObject, 0, .8f, 3).setEaseLinear().setLoopPingPong(1).setOnUpdate((float val) =>
                    {
                        gridMaterial.SetFloat("_Progress", val);
                    });
        }
        else
        {
            Debug.LogWarning("Configure the volumetric grid in the workspace to enable 3D grid special effects.");
        }

        Observable.Timer(TimeSpan.FromMilliseconds(2000)).Subscribe(_ =>
        {
            SessionManager.Instance.GoBack();
        });
    }

    private void OnDisable()
    {
        if (volumetricGrid != null)
        {
            volumetricGrid.GetComponent<MeshRenderer>().material.SetFloat("_Progress", 0);
        }
    }
}