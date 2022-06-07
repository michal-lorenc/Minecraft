using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "ScriptableObjects/Create Block", order = 1)]
public class BlockData : ScriptableObject
{
    public new string name;
    public int id;
    [Space(5)]
    public bool isTransparent;
    public bool hasGravity;
    public bool isRendered = true;
    [Space(5)]
    public bool doesNotChange = true;
    private Block block;
    [Space(5), Header("Textures")]
    public Vector2 topTextureID = new Vector2(0, 1);
    public Vector2 bottomTextureID = new Vector2(1, 2);
    public Vector2 frontTextureID = new Vector2(2, 12);
    public Vector2 leftTextureID = new Vector2(1, 2);
    public Vector2 rightTextureID = new Vector2(1, 2);
    public Vector2 backTextureID = new Vector2(1, 2);
    [Space(5), Header("Light Emmiting")]
    public bool isEmmitingLight = false;
    [Range(0, 15)] public byte lightAmount = 0;

    /// <summary>
    /// Create new block or return reference to already created block (based on 'doesNotChangeSettings')
    /// </summary>
    /// <returns></returns>
    public Block GetBlock ()
    {
        if (doesNotChange)
        {
            if (block != null)
                return block;

            block = new Block(this);
            return block;
        }
        else
        {
            return new Block(this);
        }
    }
}
