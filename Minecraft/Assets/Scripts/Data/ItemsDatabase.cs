using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "ScriptableObjects/Items/Create Items Database", order = 1)]
public class ItemsDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    public Item GetItemByID (int id)
    {
        foreach (ItemData item in items)
        {
            if (item.Id == id)
                return item.GetItem();
        }

        return null;
    }

    public Item GetItemByID(int id, int quantity)
    {
        Item item = GetItemByID(id);

        if (item == null)
            return null;

        item.quantity = quantity;
        return item;
    }
}
