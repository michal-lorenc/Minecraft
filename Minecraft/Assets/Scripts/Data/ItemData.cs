using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/Items/Create Item", order = 1)]
public class ItemData : ScriptableObject
{
    [SerializeField] private new string name;
    [SerializeField] private Sprite icon;
    [SerializeField] private int id;
    [SerializeField] private int maxQuantity = 64;
    [SerializeField] private eItemType itemType = eItemType.BUILDING;

    public string Name { get { return name; } }
    public int Id { get { return id; } }
    public Sprite Icon { get { return icon; } }
    public int MaxQuantity { get { return maxQuantity; } }
    public eItemType ItemType { get { return itemType; } }

    public Item GetItem ()
    {
        return new Item(this);
    }
}
