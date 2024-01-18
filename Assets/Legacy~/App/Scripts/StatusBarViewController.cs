using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class StatusBarViewController : MonoBehaviour
{
    [Header("Buttons")]
     public Interactable calibrateButton;
    public Interactable wellPlateSettingsButton;
    public Interactable alignmentButton;
    public Interactable genericVisualizationsButton;

    void Awake()
    {
        calibrateButton.OnClick.AsObservable().Subscribe(_ => 
        {
            //inTempScreen = true;
            SessionManager.Instance.GotoScreen(ScreenType.Calibration, true);
        }).AddTo(this);

        wellPlateSettingsButton.OnClick.AsObservable().Subscribe(_ => 
        {
            //inTempScreen = true;
            SessionManager.Instance.GotoScreen(ScreenType.WellPlateSettings, true);
        }).AddTo(this);

        genericVisualizationsButton.OnClick.AsObservable().Subscribe(_ => 
        {
            SessionState.enableGenericVisualizations.Value = !SessionState.enableGenericVisualizations.Value;
        }).AddTo(this);

        // alignmentButton.OnClick.AsObservable().Subscribe(_ =>
        // {
        //     if(!ProtocolState.LockingTriggered.Value)
        //     {
        //         ProtocolState.AlignmentTriggered.Value = !ProtocolState.AlignmentTriggered.Value;
        //         if (ProtocolState.AlignmentTriggered.Value)
        //         {
        //             AlignmentController.TriggerAlignment(specificArViews.Where(arview => arview.Key.arDefinitionType == ArDefinitionType.Model).Select(x => x.Value).ToList());
        //         } 
        //         else
        //         {
        //             AlignmentController.ResetModel(specificArViews.Where(arview => arview.Key.arDefinitionType == ArDefinitionType.Model).Select(x => x.Value).ToList());
        //         }
        //     }
        //     else
        //     {
        //         Debug.LogWarning("Cannot activate all models alignment controller while in locking");
        //     }
        // }).AddTo(this);
    }
}
