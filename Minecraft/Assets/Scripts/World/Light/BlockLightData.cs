using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockLightData
{
    public int naturalLightLevel = 0;
    public int artificialLightLevel = 0;

    public int finalLightLevelTopFace;
    public int finalLightLevelBottomFace;
    public int finalLightLevelFrontFace;
    public int finalLightLevelBackFace;
    public int finalLightLevelLeftFace;
    public int finalLightLevelRightFace;

    public int GetLightLevel ()
    {
        if (naturalLightLevel > artificialLightLevel)
            return naturalLightLevel;

        return artificialLightLevel;
    }

    public int GetFaceLightLevel (eBlockFace blockFace)
    {
        return blockFace switch
        {
            eBlockFace.TOP => finalLightLevelTopFace,
            eBlockFace.BOTTOM => finalLightLevelBottomFace,
            eBlockFace.FRONT => finalLightLevelFrontFace,
            eBlockFace.BACK => finalLightLevelBackFace,
            eBlockFace.LEFT => finalLightLevelLeftFace,
            eBlockFace.RIGHT => finalLightLevelRightFace,
            _ => throw new System.ArgumentOutOfRangeException(),
        };
    }

    public int SetFaceLightLevel(eBlockFace blockFace, int lightLevel)
    {
        return blockFace switch
        {
            eBlockFace.TOP => finalLightLevelTopFace = lightLevel,
            eBlockFace.BOTTOM => finalLightLevelBottomFace = lightLevel,
            eBlockFace.FRONT => finalLightLevelFrontFace = lightLevel,
            eBlockFace.BACK => finalLightLevelBackFace = lightLevel,
            eBlockFace.LEFT => finalLightLevelLeftFace = lightLevel,
            eBlockFace.RIGHT => finalLightLevelRightFace = lightLevel,
            _ => throw new System.ArgumentOutOfRangeException(),
        };
    }

    public float GetFaceLightIntensity (eBlockFace blockFace)
    {
        int lightLevel = GetFaceLightLevel(blockFace);
        float lightIntensity = LightLevelToIntensity(lightLevel);

        switch (blockFace)
        {
            case eBlockFace.BOTTOM:
                lightIntensity += 0.04f * lightLevel;
                break;
            case eBlockFace.FRONT:
            case eBlockFace.BACK:
                lightIntensity += 0.0133f * lightLevel;
                break;
            case eBlockFace.RIGHT:
            case eBlockFace.LEFT:
                lightIntensity += 0.0266f * lightLevel;
                break;
        }

        return lightIntensity;
    }

    public float GetLightIntensity ()
    {
        return LightLevelToIntensity(GetLightLevel());
    }

    private protected float LightLevelToIntensity (int lightLevel)
    {
        return lightLevel switch
        {
            15 => 0.0f,
            14 => 0.0625f,
            13 => 0.125f,
            12 => 0.1875f,
            11 => 0.25f,
            10 => 0.3125f,
            9 => 0.375f,
            8 => 0.4375f,
            7 => 0.5f,
            6 => 0.5625f,
            5 => 0.625f,
            4 => 0.6875f,
            3 => 0.75f,
            2 => 0.8125f,
            1 => 0.875f,
            0 => 0.9375f,
            _ => 1.0f,
        };
    }
}
