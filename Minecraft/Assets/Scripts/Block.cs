using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public BlockData BlockData { get; private set; }
  //  public BlockLightData lightData = new BlockLightData();

   // public Vector3 positionInChunk { get; private set; }

    public Block (BlockData blockData)
    {
        ReplaceBlock(blockData);
    }

    public void ReplaceBlock (BlockData blockData)
    {
        BlockData = blockData;

     //   if (BlockData.isEmmitingLight)
     //   {
     //       lightData.artificialLightLevel = BlockData.lightAmount;
     //   }
    }


    public Vector2 GetTextureForFace (eBlockFace face)
    {
        switch (face)
        {
            case eBlockFace.FRONT:
                return BlockData.frontTextureID;
            case eBlockFace.BOTTOM:
                return BlockData.bottomTextureID;
            case eBlockFace.BACK:
                return BlockData.backTextureID;
            case eBlockFace.TOP:
                return BlockData.topTextureID;
            case eBlockFace.RIGHT:
                return BlockData.rightTextureID;
            case eBlockFace.LEFT:
                return BlockData.leftTextureID;
            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }
}

public enum eBlockFace
{
    TOP = 0,
    BOTTOM = 1,
    LEFT = 2,
    RIGHT = 3,
    FRONT = 4,
    BACK = 5
}