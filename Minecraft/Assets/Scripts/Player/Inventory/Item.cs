using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public ItemData itemData;
    public int quantity = 1;

    public Item (ItemData itemData)
    {
        this.itemData = itemData;
    }

    public Item(ItemData itemData, int quantity)
    {
        this.itemData = itemData;
        this.quantity = quantity;
    }
}
