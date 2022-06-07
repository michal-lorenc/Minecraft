using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickbarUI : MonoBehaviour
{
    [SerializeField] private SlotUI[] slotUIs = new SlotUI[9];
    public static QuickbarUI signleton;

    private void Awake ()
    {
        signleton = this;
        SelectSlot(0);
    }

    public void SelectSlot (int slotID)
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i].selectionOutline != null)
            {
                slotUIs[i].selectionOutline.enabled = i == slotID;
            }
        }
    }

    public void UpdateItemsUI (Slot[] inventory)
    {
        for (int i = 0; i < 9; i++)
        {
            if (inventory[i].item != null)
            {
                ItemUI itemUI = slotUIs[i].GetSlotItem();
                itemUI.UpdateItem(inventory[i].item);
            }
            else
            {
                slotUIs[i].TryRemoveSlotItem();
            }
        }
    }
}
