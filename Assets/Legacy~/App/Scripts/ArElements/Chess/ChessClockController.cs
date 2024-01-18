using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

/// <summary>
/// Behaviour that triggers a RequestChessCompareBoards to detect a move
/// </summary>
public class ChessClockController : ArElementViewController
{
    public Transform ButtonWhite;
    public Transform ButtonBlack;
    public bool TriggerOnWhiteButton;

    [Tooltip("Detection range")]
    public float TriggerDistance = .3f;

    private bool previouslyInRange = false; 

    private void OnEnable()
    {
        // Update UI by subscribing to changes
        ChessSessionState.chessIsWhiteTurnStream.Subscribe(value =>
        {
            UpdateVisualState();
        }).AddTo(this);

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        ButtonWhite.localScale = ChessSessionState.ChessIsWhiteTurn ? new Vector3(0.03f, 0.01f, 0.03f) : new Vector3(0.03f, 0.001f, 0.03f);
        ButtonWhite.localPosition = ChessSessionState.ChessIsWhiteTurn ? new Vector3(0f, 0.1014f, 0.05f) :  new Vector3(0f, 0.0924f, 0.05f);

        ButtonBlack.localScale = !ChessSessionState.ChessIsWhiteTurn ? new Vector3(0.03f, 0.01f, 0.03f) : new Vector3(0.03f, 0.001f, 0.03f);
        ButtonBlack.localPosition = !ChessSessionState.ChessIsWhiteTurn ? new Vector3(0f, 0.1014f, -0.05f) : new Vector3(0f, 0.0924f, -0.05f);
   }

    public override void Initialize(ArDefinition arDefinition, List<TrackedObject> trackedObjects)
    {
        base.Initialize(arDefinition, trackedObjects);
    }

    public void Update()
    {
        // Note HandJointInjector in the TrackedObjectServices GameObject must be enabled
        // It will add the HoloLens fingertips as trackedobjects

        if (TrackedObjects != null)
        {
            // Find fingers of interest
            var fingerTips = (from to in SessionState.TrackedObjects
                              where to.label == "LeftIndexTip" || to.label == "RightIndexTip"
                              select to);

            var targetLocation = TriggerOnWhiteButton ? new Vector3(ButtonWhite.position.x, 0, ButtonWhite.position.z) : new Vector3(ButtonBlack.position.x, 0, ButtonBlack.position.z);

            // Determine if one of the fingers is close enough
            bool inRange = false;
            if (fingerTips.Count() > 0)
            {
                foreach (var fingerTip in fingerTips)
                {
                    // Note, using XZ based distance
                    if (Vector3.Distance(targetLocation, new Vector3(fingerTip.position.x, 0, fingerTip.position.z)) < TriggerDistance)
                    {
                        inRange = true;
                    }
                }
            }

            if ((ChessSessionState.ChessIsWhiteTurn == TriggerOnWhiteButton) && inRange && !previouslyInRange)
            {
                ServiceRegistry.GetService<IChessControl>()?.RequestChessCompareBoards();
            }

            previouslyInRange = inRange;
        }
    }
}
