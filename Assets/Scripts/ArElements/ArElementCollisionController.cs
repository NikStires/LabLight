using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class handles all collisions for ArElement it is attatched to
/// This script requires the following:
///     A rigidbody with Is Kinimatic selected
///     A Collider with Is Trigger selected
/// </summary>
public class ArElementCollisionController : MonoBehaviour
{
    public TrackedObject ThisTrackedObject;
    public string ThisObjectName;
    public ArDefinitionType ArType;

    public Material WarningMat;
    public Material DefaultMat;

    public string otherObjectName;
    // Start is called before the first frame update
    void Start()
    {
        ArType = this.gameObject.GetComponent<WorldPositionController>().GetArDefinitionType();

        //operates differently depending on ArElement Type
        switch (ArType)
        {
            //CONTAINER TYPE
            //if this is a container ar element then set the collider to the size of the tracked object class
            case (ArDefinitionType.Container):

                //Get the object this container is tracked to
                if (this.GetComponent<ArElementViewController>() != null)
                {
                    ThisTrackedObject = this.GetComponent<ArElementViewController>().TrackedObjects[0];
                    ThisObjectName = ThisTrackedObject.label;
                }

                //Set collider size based on tracked object
                if (ThisObjectName == "96 Well Plate")
                {
                    this.gameObject.GetComponent<BoxCollider>().size = new Vector3(0.125f, 0.03f, 0.08f);
                }
                else if (ThisObjectName == "50mL Tube Rack")
                {
                    this.gameObject.GetComponent<BoxCollider>().size = new Vector3(0.09f, 0.085f, 0.045f);
                }
                else if (ThisObjectName == "50mL Tube")
                {
                    this.gameObject.GetComponent<BoxCollider>().size = new Vector3(0.03f, 0.115f, 0.03f);
                }
                else if (ThisObjectName == "Micropipette")
                {
                    this.gameObject.GetComponent<BoxCollider>().size = new Vector3(0.05f, 0.25f, 0.04f);
                }
                break;

            //MODEL TYPE
            //if this is a model type
            case (ArDefinitionType.Model):
                ThisObjectName = this.GetComponent<ModelElementViewController>().ObjectName;
                break;

            default:
                break;
        }
        //Debug.Log(ThisObjectName + " Added to scene");
    }

    private void OnTriggerEnter(Collider other)
    {
        //check if collision is with a Container ArElement
        if(other.gameObject.GetComponent<ContainerElementViewController>() != null)
        {
            //Get Identity of colliding Container ArElement
            otherObjectName = other.gameObject.GetComponent<ContainerElementViewController>().TrackedObjects[0].label;
            
            //Debug.Log(ThisObjectName + " colliding with " + otherObjectName);

            //Do stuff depending on what THIS ArElement is
            if(ThisObjectName == otherObjectName)
            {
                return;
            }
            if (ThisObjectName == "96 Well Plate")
            {
                if (otherObjectName == "Micropipette")
                {
                    //Debug.Log("Started Pipetting");
                }
            }
        }

        //check if collision is with a Model ArElement
        if (other.gameObject.GetComponent<ModelElementViewController>() != null)
        {
            //Get identity of colliding Model ArElement
            otherObjectName = other.gameObject.GetComponent<ModelElementViewController>().ObjectName;
            //Debug.Log(ThisObjectName + " colliding with " + otherObjectName);
        }

//        //check if collision is with palm of hand (object picked up)
//        if (other.gameObject.GetComponent<JointKinematicBody>() != null)
//        {
//            //Do stuff depending on what THIS ArElement is
//            if (ThisObjectName == "96 Well Plate")
//            {
//                //Debug.LogWarning("Possible 96 Well Plate Contamination");
///*                if(ArType == ArDefinitionType.Model)
//                {
//                    this.transform.GetChild(0).GetComponent<Renderer>().material = WarningMat;
//                }*/
//            }
//            if(ThisTrackedObject.label == "Micropipette")
//            {
//                //Debug.Log("Micropipette picked up");
//            }
//        }


        //NS TODO: Other ArElement types
    }

    private void OnTriggerExit(Collider other)
    {
        //check if exit collision is with a Container ArElement
        if (other.gameObject.GetComponent<ContainerElementViewController>() != null)
        {
            //Get Identity of colliding Container ArElement
            otherObjectName = other.gameObject.GetComponent<ContainerElementViewController>().TrackedObjects[0].label;

            //Debug.Log(ThisObjectName + " exiting collision with " + otherObjectName);

            //Do stuff depending on what THIS ArElement is
            if (ThisObjectName == otherObjectName)
            {
                return;
            }
            if (ThisObjectName == "96 Well Plate")
            {
                if (otherObjectName == "Micropipette")
                {
                    //Debug.Log("Finished Pipetting");
                }
            }
        }

        //check if exit collision is with a Model ArElement
        if (other.gameObject.GetComponent<ModelElementViewController>() != null)
        {
            //Get identity of colliding Model ArElement
            otherObjectName = other.gameObject.GetComponent<ModelElementViewController>().ObjectName;
            //Debug.Log(ThisObjectName + " exiting collision with " + otherObjectName);
        }

//        //check if collision is with palm of hand (object picked up)
//        if (other.gameObject.GetComponent<JointKinematicBody>() != null)
//        {
//            //Do stuff depending on what THIS ArElement is
//            if (ThisObjectName == "96 Well Plate")
//            {
///*                if (ArType == ArDefinitionType.Model)
//                {
//                    this.transform.GetChild(0).GetComponent<Renderer>().material = DefaultMat;
//                }*/
//            }
//            if (ThisTrackedObject.label == "Micropipette")
//            {
//                //Debug.Log("Micropipette set down");
//            }
//        }

        //NS TODO: Other ArElement types
    }
}
