using UnityEngine;

public interface ISlotProvider 
{
    public void ReturnSlot(Transform transform);
    public Transform GetFreeSlot();
}
