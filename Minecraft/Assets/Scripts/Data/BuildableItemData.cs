using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/Items/Create Buildable Item", order = 1)]
public class BuildableItemData : ItemData
{
    [Header("Buildable"), Space(5)]
    [SerializeField] private BlockData block;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] public BlockData Block { get { return block; } }
    [SerializeField] public AudioClip PlaceSound { get { return placeSound; } }
}
