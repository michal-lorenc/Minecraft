using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public ItemsDatabase itemsDatabase;
    public Slot[] inventory = new Slot[20];
    public int selectedInventorySlot = 0;
    private int maxSelectedInventorySlot = 8;
    private string inputString;

    private void Start ()
    {
        LoadInventory();
    }

    private void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            selectedInventorySlot--;

            if (selectedInventorySlot < 0)
            {
                selectedInventorySlot = maxSelectedInventorySlot;
            }

            OnSelectedSlotChanged();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            selectedInventorySlot++;

            if (selectedInventorySlot > maxSelectedInventorySlot)
            {
                selectedInventorySlot = 0;
            }

            OnSelectedSlotChanged();
        }

        inputString = Input.inputString;

        if (inputString == "1" || inputString == "2" || inputString == "3" || inputString == "4" || inputString == "5" || inputString == "6" || inputString == "7" || inputString == "8" || inputString == "9")
        {
            selectedInventorySlot = int.Parse(inputString) - 1;
            OnSelectedSlotChanged();
        }
    }

    private void OnSelectedSlotChanged ()
    {
        QuickbarUI.signleton.SelectSlot(selectedInventorySlot);
    }

    private void OnItemChanged ()
    {
        QuickbarUI.signleton.UpdateItemsUI(inventory);
    }

    private void LoadInventory ()
    {
        for (int i = 0; i < 20; i++)
        {
            inventory[i] = new Slot();
        }

        inventory[0].item = itemsDatabase.GetItemByID(2, 33);
        inventory[1].item = itemsDatabase.GetItemByID(1, 55);
        OnItemChanged();
    }

    public Item UseSelectedItem ()
    {
        Item item = inventory[selectedInventorySlot].item;

        if (item == null)
            return null;

        item.quantity--;

        if (item.quantity <= 0)
            inventory[selectedInventorySlot].item = null;

        OnItemChanged();

        return item;
    }

    public Item GetSelectedItem ()
    {
        Item item = inventory[selectedInventorySlot].item;

        return item;
    }

}
