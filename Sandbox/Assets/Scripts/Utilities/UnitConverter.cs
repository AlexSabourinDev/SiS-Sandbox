using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitConverter
{

    //More resources: https://www.youtube.com/watch?v=6-T2tfM0IzU <- cat long jump
    // https://www.youtube.com/watch?v=bq47C-jBF4g <- cat high jump
    // https://www.youtube.com/watch?v=xZqeUn8dvOc <- effortless cheetah jump

    const float CAT_LAND_SPEED_KMPH = 47.0f; //km/h
    const float CAT_LAND_SPEED = 13.0f; //m/s
    const float CAT_HEIGHT = 0.3f; //metres
    const float CAT_LENGTH = 0.46f; //metres
    const float CAT_JUMP_HEIGHT = 1.8f; //metres
    const float CAT_WEIGHT = 4.0f; //kilograms
    const float CAT_VOLUME = 2.35f; //litres
    
    const float CAT_DENSITY = CAT_WEIGHT / CAT_VOLUME; //kg/L

    static public float ConvertMetresToCatHeights (float metres)
    {
        return metres / CAT_HEIGHT;
    }

    static public float ConvertCatHeightsToMetres (float catHeights)
    {
        return catHeights * CAT_HEIGHT;
    }

    static public float ConvertMetresToCatLengths (float metres)
    {
        return metres / CAT_LENGTH;
    }

    static public float ConvertCatLengthsToMetres(float catLengths)
    {
        return catLengths * CAT_HEIGHT;
    }

    static public float ConvertMetresToCatJumpHeight(float metres)
    {
        return metres / CAT_JUMP_HEIGHT;
    }

    static public float ConvertCatJumpsToMetres(float catJumpHeights)
    {
        return catJumpHeights * CAT_HEIGHT;
    }
    
}
