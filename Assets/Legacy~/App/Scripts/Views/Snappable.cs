using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Linq;


/// <summary>
/// The object with this component must be have a title bar prefab as a child
/// 
/// The Child is the transform that should be snapped to the main LLPanel
/// The Snap Offset will determine the position relative to the LLPanel
/// The Threshold is how close the child must be to the LLPanel to snap
/// 
/// Events needed on title bar:
/// OnPointerClicked: Snappable.Grabbed
/// OnPointerDragged: Snappable.UnSnap, Snappable.DrawFeedback
/// </summary>
public class Snappable : MonoBehaviour
{

    private Transform LLPanel;
    private Vector3 PrevLLPanelPos;
    private bool Snapped = false;
    private Vector3 LLPanelScale;
    private SnapController SlotManager; //keeps track of filled slots
    private int slot; // 0:None 1:Right 2:Left
    private Transform feedback;

    public float Threshold;
    public Transform ChildBG;
    public Material HighlightMaterial;

    // Start is called before the first frame update
    void Start()
    {
        //get main LLPanel and its starting position/scale
        LLPanel = GameObject.Find("ProcedurePanel").transform;
        PrevLLPanelPos = LLPanel.position;
        LLPanelScale = LLPanel.lossyScale;

        SlotManager = LLPanel.GetComponent<SnapController>();
        feedback = Instantiate(ChildBG, LLPanel.parent);
        
        foreach(Transform childBG in feedback)
        {
            childBG.GetComponent<Renderer>().material = HighlightMaterial;
        }

        feedback.gameObject.SetActive(false);

        Snap();
    }

    void OnAwake()
    {
        if (Snapped)
        {
            Snap();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if snapped to LLPanel and it has moved snap again
        if(Snapped & (LLPanel.position != PrevLLPanelPos | LLPanel.lossyScale != LLPanelScale | LLPanel.rotation != transform.rotation))
        {
            Snap();
        }
    }

    public void Grabbed()
    {
        //Scale threshold with main LLPanel for consistent UX
        float ScaledThreshold = Threshold * LLPanelScale.x;
        //if dragged into threshold snap
        if(Vector3.Distance(transform.localPosition, LLPanel.localPosition) < ScaledThreshold)
        {
            feedback.gameObject.SetActive(false);
            Snap();
        }
    }

    void Snap()
    {
        Snapped = true;
        //update LLPanel position and scale
        PrevLLPanelPos = LLPanel.position;
        LLPanelScale = LLPanel.lossyScale;
        
        //Get snap positions
        Vector3 RightSnap = LLPanel.localPosition + LLPanel.right * -LLPanelScale.x / 2.5f;
        RightSnap -= LLPanel.eulerAngles;
        Vector3 LeftSnap = LLPanel.localPosition + LLPanel.right * LLPanelScale.x / 2.5f;
        RightSnap += LLPanel.eulerAngles;
        Vector3 ClosestSnap;

        //If not already snapped find closest position
        if (slot == 0)
        {
           ClosestSnap = GetClosestPosition(RightSnap, LeftSnap);
        }
        
        //Snap to desired slot 0:Null 1:Right 2:Left
        switch (slot)
        {
            case 0:
                break;
            case 1:
                SlotManager.RightSlotFilled = true;
                transform.localPosition = RightSnap;
                transform.rotation = LLPanel.rotation;
                transform.localScale = LLPanelScale * 1.25f;
                break;
            case 2:
                SlotManager.LeftSlotFilled = true;
                transform.localPosition = LeftSnap;
                transform.rotation = LLPanel.rotation;
                transform.localScale = LLPanelScale * 1.25f;
                break;
            default:
                break;
        }

        if(transform.GetComponent<RadialView>() != null)
        {
            transform.GetComponent<RadialView>().enabled = false;
        }
    }

    private Vector3 GetClosestPosition(Vector3 Right, Vector3 Left)
    {
        //calculate distances
        float DistanceRight = Vector3.Distance(transform.localPosition, Right);
        float DistanceLeft = Vector3.Distance(transform.localPosition, Left);

        //find min and return corrosponding position
        if(DistanceRight < DistanceLeft & !SlotManager.RightSlotFilled)
        {
            slot = 1;
            return Right;
        }
        else if(DistanceLeft < DistanceRight & !SlotManager.LeftSlotFilled)
        {
            slot = 2;
            return Left;
        }
        else
        {
            slot = 0;
            return Vector3.zero;
        }
    }

    public void UnSnap()
    {
        Snapped = false;
        switch (slot)
        {
            case 0:
                break;
            case 1:
                SlotManager.RightSlotFilled = false;
                break;
            case 2:
                SlotManager.LeftSlotFilled = false;
                break;
            default:
                break;
        }
        slot = 0;

        if(transform.GetComponent<RadialView>() != null)
        {
            transform.GetComponent<RadialView>().enabled = true;
        }
    }

    public void DrawFeedback()
    {
        //Scale threshold with main LLPanel for consistent UX
        float ScaledThreshold = Threshold * LLPanelScale.x;
        //if dragged into threshold snap
        if (Vector3.Distance(transform.localPosition, LLPanel.localPosition) < ScaledThreshold)
        {
            //update LLPanel position and scale
            PrevLLPanelPos = LLPanel.position;
            LLPanelScale = LLPanel.lossyScale;

            //Get snap positions
            Vector3 RightSnap = LLPanel.localPosition + LLPanel.right * -LLPanelScale.x / 2.5f;
            Vector3 LeftSnap = LLPanel.localPosition + LLPanel.right * LLPanelScale.x / 2.5f;

            Vector3 ClosestSnap = GetClosestPosition(RightSnap, LeftSnap);

            if(ClosestSnap != Vector3.zero)
            {

                feedback.localPosition = ClosestSnap;
                feedback.rotation = LLPanel.rotation;
                feedback.localScale = ChildBG.lossyScale;
                feedback.gameObject.SetActive(true);
            }
        }
        else
        {
            feedback.gameObject.SetActive(false);
        }
    }
}