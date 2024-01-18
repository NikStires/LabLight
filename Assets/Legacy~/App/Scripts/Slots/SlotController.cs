using System.Collections.Generic;
using UnityEngine;

public class SlotController : MonoBehaviour, ISlotProvider
{
    [SerializeField]
    [Tooltip("Child transforms will be used a s free slots")]
    private Transform _slotContainer;

    /// <summary>
    /// Stores availability of the slot
    /// </summary>
    private Dictionary<Transform, bool> _freeSlotDictionary = new Dictionary<Transform, bool>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i=0; i < _slotContainer.childCount; i++)
        {
            _freeSlotDictionary[_slotContainer.GetChild(i)] = true;
        }
    }

    private void OnEnable()
    {
        ServiceRegistry.RegisterService<ISlotProvider>(this);
    }

    private void OnDisable()
    {
        ServiceRegistry.UnRegisterService<ISlotProvider>();
    }

    public Transform GetFreeSlot()
    {
        // Return first free slot
        foreach (var slot in _freeSlotDictionary)
        {
            if (slot.Value)
            {
                _freeSlotDictionary[slot.Key] = false;
                return slot.Key;
            }
        }
        return null;
    }

    public void ReturnSlot(Transform transform)
    {
        _freeSlotDictionary[transform] = true;
    }
}
