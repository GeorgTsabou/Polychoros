using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMath : MonoBehaviour {

    public float lerpScale(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue)
    {

        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        if (NewValue < NewMin)
        {
            NewValue = NewMin;
        }
        else if (NewValue > NewMax)
        {
            NewValue = NewMax;
        }
        return (NewValue);
    }


    public Color colorLerpScale(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue)
    {
        Color c;
        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        if (NewValue < NewMin)
        {
            NewValue = NewMin;
        }
        else if (NewValue > NewMax)
        {
            NewValue = NewMax;
        }
        
        return (new Color(NewValue, NewValue, NewValue));
    }
}
