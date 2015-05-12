using System;
using UnityEngine;

public class EmptyPerformance : Performance
{
    public override void Cast(Transform target)
    {
        Debug.Log("This is an empty slot");
    }

    public override void SetCurrentPerformance()
    {

    }

    public override void Damage(Transform target)
    {

    }
}