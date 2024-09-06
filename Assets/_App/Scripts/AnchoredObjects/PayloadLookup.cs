using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PayloadLookup", menuName = "ScriptableObjects/PayloadLookup", order = 2)]
public class PayloadLookup : ScriptableObject
{
    public SpatialNoteController spatialNote;
    public HazardZone hazardZone;

    public AnchorPayloadController FindPrefabToCreate(PayloadType payloadType)
    {
        Debug.Log("FindPrefabToCreate " + payloadType);

        switch (payloadType)
        {
            case PayloadType.Note:
                return spatialNote;
            case PayloadType.HazardZone:
                return hazardZone;
            default:
                return null;
        }
    }
}
